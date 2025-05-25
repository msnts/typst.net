using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Typst.Net;
using Typst.Net.Examples.Api.Extensions;

namespace Typst.Net.Examples.Api.Endpoints.CompilerEndpoints;

public static class CompilerEndpoints
{
    public static void MapCompilerEndpoints(this WebApplication app)
    {
        // POST /compile/{outputFormat}
        // Expects the raw Typst document content in the request body.
        // outputFormat should be "pdf", "svg", or "png" (case-insensitive).
        app.MapPost("/compile/{outputFormatString}", HandleTypstCompilation);
    }

    private static async Task<IResult> HandleTypstCompilation(string outputFormatString, [FromBody] JsonDocument data, ITypstCompiler compiler, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OutputFormat>(outputFormatString, ignoreCase: true, out var outputFormat))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "outputFormatString", new[] { $"Invalid format. Must be one of: {string.Join(", ", Enum.GetNames<OutputFormat>())}" } }
            });
        }

        var compileOptions = new TypstCompileOptions
        {
            Format = outputFormat,
            Data = data.RootElement.GetRawText() // Pass the JSON data to the compiler
        };

        using StreamReader inputStream = new("example.typ");

        try
        {
            TypstResult result = await compiler.CompileAsync(inputStream.BaseStream, compileOptions, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Typst compilation completed successfully. Details:{details}", result.Details);

                var fileDownloadName = $"output.{outputFormat.ToString().ToLowerInvariant()}";

                return Results.File(result.Output!, outputFormat.ToContentType(), fileDownloadName);
            }

            return Results.Problem(
                type: "Compilation failed.",
                title: result.Error.Description,
                detail: result.Details,
                statusCode: result.Error.Code == ErrorCode.CompilationError
                    ? StatusCodes.Status422UnprocessableEntity 
                    : StatusCodes.Status500InternalServerError);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
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
}