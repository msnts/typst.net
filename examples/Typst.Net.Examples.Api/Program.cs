using Typst.Net.Core;
using Typst.Net.Core.Configuration;

using Typst.Net.Examples.Api.Endpoints.CompilerEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddTypst();

builder.Services.AddOptions<TypstOptions>()
    .Bind(builder.Configuration.GetSection(TypstOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection(); // Optional: Use HTTPS

// --- Minimal API Endpoint ---
app.MapCompilerEndpoints();

app.Run();