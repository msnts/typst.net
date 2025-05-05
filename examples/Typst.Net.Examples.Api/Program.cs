using Microsoft.AspNetCore.Mvc;
using Typst.Net.Core; // Base namespace for your library
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Exceptions; // Namespace for TypstOptions

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddTypstCompiler();

builder.Services.AddOptions<TypstOptions>()
    .Bind(builder.Configuration.GetSection(TypstOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
    

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Optional: Use HTTPS

// --- Minimal API Endpoint ---

// POST /compile/{outputFormat}
// Expects the raw Typst document content in the request body.
// outputFormat should be "pdf", "svg", or "png" (case-insensitive).
app.MapPost("/compile/{outputFormatString}", HandleTypstCompilation)
   .WithName("CompileTypstDocument")
   .WithDescription("Compiles a Typst document provided in the request body to the specified format (pdf, svg, png).")
   .Produces(StatusCodes.Status200OK, contentType: "application/pdf") // Add Produces for other types too
   .Produces(StatusCodes.Status200OK, contentType: "image/svg+xml")
   .Produces(StatusCodes.Status200OK, contentType: "image/png")
   .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
   .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


app.Run();

// --- Endpoint Handler Logic ---
async Task<IResult> HandleTypstCompilation(
    string outputFormatString, // From route parameter
    HttpRequest request,       // Access request body stream
    ITypstCompiler compiler,   // Injected service
    ILogger<Program> logger)   // Logger for the endpoint
{
    logger.LogInformation("Received compilation request for format: {FormatString}", outputFormatString);

    // 1. Parse and validate output format from route
    if (!Enum.TryParse<OutputFormat>(outputFormatString, ignoreCase: true, out var outputFormat))
    {
        logger.LogWarning("Invalid output format requested: {FormatString}", outputFormatString);
        return Results.ValidationProblem(new Dictionary<string, string[]> {
            { "outputFormatString", new[] { $"Invalid format. Must be one of: {string.Join(", ", Enum.GetNames<OutputFormat>())}" } }
        });
    }

    // 2. Prepare compile options (add more from query params or headers if needed)
    var compileOptions = new TypstCompileOptions
    {
        Format = outputFormat
        // Example: Read RootDirectory from query param if provided
        // RootDirectory = request.Query.TryGetValue("root", out var rootDir) ? rootDir.ToString() : null
    };

    // 3. Get input stream (request body) and cancellation token
    var inputStream = request.Body;
    var cancellationToken = request.HttpContext.RequestAborted; // Use request cancellation

    try
    {
        // 4. Call the compiler service
        logger.LogInformation("Invoking TypstCompiler for format {Format}...", outputFormat);
        TypstResult result = await compiler.CompileAsync(inputStream, compileOptions, cancellationToken);
        logger.LogInformation("Typst compilation successful. Returning {Bytes} bytes.", result.OutputData.Length);

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
             logger.LogWarning("Typst compilation produced stderr output (might contain warnings):\n{Stderr}", result.StandardError);
        }

        // 5. Determine Content-Type and filename for the response
        string contentType = outputFormat switch
        {
            OutputFormat.Pdf => "application/pdf",
            OutputFormat.Svg => "image/svg+xml",
            OutputFormat.Png => "image/png",
            _ => "application/octet-stream" // Fallback
        };
        string fileDownloadName = $"output.{outputFormat.ToString().ToLowerInvariant()}";

        // 6. Return the MemoryStream as a File result
        // The MemoryStream from the result will be properly disposed by the FileResult infrastructure.
        return Results.File(result.OutputData, contentType, fileDownloadName);
    }
    catch (TypstCompilationException ex)
    {
        logger.LogError(ex, "Typst compilation failed. Stderr:\n{Stderr}", ex.StandardError);
        // Return 400 Bad Request for compilation errors, as they often relate to the input document
        return Results.BadRequest(new {
             Error = "Typst compilation failed.",
             Details = ex.Message,
             StandardError = ex.StandardError
         });
    }
    catch (TypstException ex) // Catch other library-specific errors (Process, Configuration)
    {
         logger.LogError(ex, "Typst library error during compilation.");
         // Return 500 Internal Server Error for process/config issues
         return Results.Problem(
             title: "Typst processing error.",
             detail: ex.Message,
             statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (OperationCanceledException ex)
    {
         logger.LogWarning(ex, "Typst compilation request was canceled.");
         // Standard way to handle cancellation is often to just return (ASP.NET Core handles logging etc.)
         // Or return a specific status code if desired, like 499 Client Closed Request (non-standard) or 503 Service Unavailable.
         // Returning Problem might be misleading. Let's return a simple status code for cancellation.
         return Results.StatusCode(StatusCodes.Status499ClientClosedRequest); // Or just return Results.Ok() or nothing if appropriate
    }
    catch (Exception ex) // Catch any other unexpected errors
    {
        logger.LogError(ex, "An unexpected error occurred during Typst compilation.");
        return Results.Problem(
            title: "An unexpected server error occurred.",
            detail: "Failed to process the Typst document due to an internal error.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}