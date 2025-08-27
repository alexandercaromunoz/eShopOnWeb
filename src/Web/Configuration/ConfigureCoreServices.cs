//using Microsoft.eShopWeb.Infrastructure.Logging;
//using Microsoft.eShopWeb.Infrastructure.Services;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // All domain/infrastructure services removed for pure UI layer.
        return services;
    }
}
