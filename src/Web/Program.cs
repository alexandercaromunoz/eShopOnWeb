using System.Net.Mime;
using Ardalis.ListStartupServices;
using Azure.Identity;
using BlazorAdmin;
using BlazorAdmin.Services;
using Blazored.LocalStorage;
using BlazorShared;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.eShopWeb.Web;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.ApplicationCore.Services; // UriComposer
using Microsoft.eShopWeb; // CatalogSettings namespace
using Microsoft.EntityFrameworkCore; // added earlier

var builder = WebApplication.CreateBuilder(args);

var baseUrlsSection = builder.Configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
builder.Services.Configure<BaseUrlConfiguration>(baseUrlsSection);

// Catalog settings + Uri composer
var catalogSettingsSection = builder.Configuration.GetSection(nameof(CatalogSettings));
var catalogSettings = catalogSettingsSection.Get<CatalogSettings>() ?? new CatalogSettings();
builder.Services.AddSingleton(catalogSettings);
builder.Services.AddSingleton<IUriComposer>(new UriComposer(catalogSettings));

// Infrastructure (DbContexts)
Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Repositories & services
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IBasketQueryService, Microsoft.eShopWeb.Infrastructure.Data.Queries.BasketQueryService>();
builder.Services.AddScoped<IBasketService, BasketService>(); // added registration for basket domain service
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>)); // register generic application logger
builder.Services.AddScoped<IOrderService, OrderService>(); // register order service

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddRouting(options =>
{
    options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
});

builder.Services.AddMvc(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Basket/Checkout");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("api_health_check", tags: new[] { "apiHealthCheck" })
    .AddCheck<HomePageHealthCheck>("home_page_health_check", tags: new[] { "homePageHealthCheck" });

builder.Services.Configure<ServiceConfig>(config =>
{
    config.Services = new List<ServiceDescriptor>(builder.Services);
    config.Path = "/allservices";
});

// Blazor Admin
var baseUrlConfig = baseUrlsSection.Get<BaseUrlConfiguration>();
builder.Services.AddScoped<HttpClient>(s => new HttpClient { BaseAddress = new Uri(baseUrlConfig!.WebBase) });

builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<HttpService>();
builder.Services.AddBlazorServices();

var app = builder.Build();

// Seed Identity database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var identityCtx = services.GetRequiredService<AppIdentityDbContext>();
        // Ensure identity database schema only if relational
        if (identityCtx.Database.IsRelational())
        {
            await identityCtx.Database.MigrateAsync();
        }
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await AppIdentityDbContextSeed.SeedAsync(identityCtx, userManager, roleManager);

        // Seed catalog data so product names are available to BasketViewModelService
        var catalogCtx = services.GetRequiredService<CatalogContext>();
        if (catalogCtx.Database.IsRelational())
        {
            await catalogCtx.Database.MigrateAsync();
        }
        var catLogger = services.GetRequiredService<ILogger<CatalogContextSeed>>();
        await CatalogContextSeed.SeedAsync(catalogCtx, catLogger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the databases");
    }
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller:slugify=Home}/{action:slugify=Index}/{id?}");
app.MapRazorPages();
app.MapHealthChecks("home_page_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("homePageHealthCheck") });
app.MapHealthChecks("api_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("apiHealthCheck") });
app.MapFallbackToFile("index.html");

app.Run();
