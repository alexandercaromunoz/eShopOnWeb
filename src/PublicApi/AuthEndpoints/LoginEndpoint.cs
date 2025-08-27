using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.AuthEndpoints;

public class LoginEndpoint : IEndpoint<IResult, LoginRequest, UserManager<ApplicationUser>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (LoginRequest request, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) =>
        {
            var endpoint = new LoginEndpoint();
            return await endpoint.HandleLoginAsync(request, userManager, signInManager);
        }).Produces<LoginResponse>()
          .WithTags("AuthEndpoints");
    }

    // Interface requirement (unused path)
    public Task<IResult> HandleAsync(LoginRequest request, UserManager<ApplicationUser> userManager) => Task.FromResult(Results.BadRequest() as IResult);

    private async Task<IResult> HandleLoginAsync(LoginRequest request, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        var user = await userManager.FindByNameAsync(request.UserName) ?? await userManager.FindByEmailAsync(request.UserName);
        if (user == null) return Results.Unauthorized();

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid) return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AuthorizationConstants.JWT_SECRET_KEY));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var response = new LoginResponse(request.CorrelationId())
        {
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            Token = tokenString,
            ExpiresAt = token.ValidTo
        };
        return Results.Ok(response);
    }
}
