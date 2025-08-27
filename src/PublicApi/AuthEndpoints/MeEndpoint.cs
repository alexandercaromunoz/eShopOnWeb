using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.AuthEndpoints;

public class MeEndpoint : IEndpoint<IResult, BaseRequest>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/me", [Authorize] (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            var response = new UserInfoResponse(Guid.NewGuid())
            {
                UserName = user.Identity?.Name ?? string.Empty,
                Roles = user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList(),
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false
            };
            return Results.Ok(response);
        }).Produces<UserInfoResponse>()
          .WithTags("AuthEndpoints");
    }

    public Task<IResult> HandleAsync(BaseRequest request) => Task.FromResult(Results.BadRequest() as IResult);
}
