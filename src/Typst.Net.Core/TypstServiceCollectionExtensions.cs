using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Typst.Net.Core.Configuration;

namespace Typst.Net.Core;

/// <summary>
/// Provides extension methods for IServiceCollection to register Typst services.
/// </summary>
public static class TypstServiceCollectionExtensions
{
    /// <summary>
    /// Adds Typst services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddTypstCompiler(this IServiceCollection services)
    {
        services.AddOptions<TypstOptions>();
        services.AddSingleton<IProcessWrapper, ProcessWrapper>();
        services.AddSingleton<ITypstCompiler, TypstCompiler>();
        return services;
    }
}
