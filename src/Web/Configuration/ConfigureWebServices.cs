using MediatR;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.eShopWeb.Web.Infrastructure.ApiClients;
using BlazorShared;
using Blazored.LocalStorage;
using Microsoft.eShopWeb.Web.Infrastructure.Http;
using Microsoft.eShopWeb.Web.Services;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureWebServices
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ConfigureWebServices).Assembly));

        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<Http401Handler>();

        services.AddHttpClient("PublicApiAnon", (sp, client) =>
        {
            var urls = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BaseUrlConfiguration>>().Value;
            client.BaseAddress = new Uri(urls.ApiBase);
        });

        services.AddHttpClient<ICatalogViewModelService, CatalogApiClient>((sp, client) =>
        {
            var urls = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BaseUrlConfiguration>>().Value;
            client.BaseAddress = new Uri(urls.ApiBase);
        });

        // Server-side basket service
        services.AddScoped<IBasketViewModelService, BasketViewModelService>();

        services.AddHttpClient<IOrderApiClient, OrderApiClient>((sp, client) =>
        {
            var urls = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BaseUrlConfiguration>>().Value;
            client.BaseAddress = new Uri(urls.ApiBase);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .AddHttpMessageHandler<Http401Handler>();

        services.AddHttpClient<ITokenAuthApiClient, TokenAuthApiClient>((sp, client) =>
        {
            var urls = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BaseUrlConfiguration>>().Value;
            client.BaseAddress = new Uri(urls.ApiBase);
        });

        services.AddHttpClient<IUserInfoProvider, UserInfoProvider>((sp, client) =>
        {
            var urls = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BaseUrlConfiguration>>().Value;
            client.BaseAddress = new Uri(urls.ApiBase);
        }).AddHttpMessageHandler<AuthHeaderHandler>()
          .AddHttpMessageHandler<Http401Handler>();

        services.AddBlazoredLocalStorage();

        return services;
    }
}
