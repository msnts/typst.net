using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Process;

namespace Typst.Net.Core;

/// <summary>
/// Extensions for adding Typst services to the service collection.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TypstServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Typst services to the specified <see cref="IServiceCollection"/>.
    /// /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This method registers the Typst process factory, compiler, and options configuration.
    /// It binds the configuration section named <c>Typst</c> to the <see cref="TypstOptions"/> class,
    /// validates the options using data annotations, and ensures validation occurs on application startup.
    /// </remarks>
    public static IServiceCollection AddTypst(this IServiceCollection services)
    {
        services
            .AddSingleton<ITypstProcessFactory, TypstProcessFactory>()
            .AddScoped<ITypstCompiler, TypstCompiler>()
            .AddOptions<TypstOptions>()
            .BindConfiguration(TypstOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
