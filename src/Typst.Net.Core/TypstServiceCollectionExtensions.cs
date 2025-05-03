using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Typst.Net.Core.Configuration;

namespace Typst.Net.Core;

public static class TypstServiceCollectionExtensions
{
    public static IServiceCollection AddTypst(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<TypstOptions>, TypstOptionsValidation>();
        services.AddScoped<ITypstCompiler, TypstCompiler>();

        return services;
    }
}
