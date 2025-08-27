using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.Infrastructure.Identity;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.AuthEndpoints;

public class RegisterEndpoint : IEndpoint<IResult, RegisterRequest, UserManager<ApplicationUser>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (RegisterRequest request, UserManager<ApplicationUser> userManager) =>
        {
            var endpoint = new RegisterEndpoint();
            return await endpoint.HandleAsync(request, userManager);
        }).Produces<LoginResponse>()
          .WithTags("AuthEndpoints");
    }

    public async Task<IResult> HandleAsync(RegisterRequest request, UserManager<ApplicationUser> userManager)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing != null) return Results.BadRequest("User already exists");
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(string.Join(";", result.Errors.Select(e => e.Description)));
        }
        var loginEp = new LoginEndpoint();
        return await loginEp.HandleAsync(new LoginRequest { UserName = request.Email, Password = request.Password }, userManager);
    }
}
