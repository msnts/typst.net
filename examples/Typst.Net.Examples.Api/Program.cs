using Typst.Net;

using Typst.Net.Examples.Api.Endpoints.CompilerEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddTypst();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection(); // Optional: Use HTTPS

// --- Minimal API Endpoint ---
app.MapCompilerEndpoints();

app.Run();