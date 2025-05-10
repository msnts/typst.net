using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Typst.Net.Core.Process;

namespace Typst.Net.Core;

/// <summary>
/// Extension methods for configuring Typst services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TypstServiceCollectionExtensions
{
    /// <summary>
    /// Adds Typst services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypst(this IServiceCollection services)
    {
        services.AddSingleton<ITypstProcessFactory, TypstProcessFactory>();
        services.AddScoped<ITypstCompiler, TypstCompiler>();
        return services;
    }
}
