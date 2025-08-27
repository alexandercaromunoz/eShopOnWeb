using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

namespace Microsoft.eShopWeb.Web.Infrastructure.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenAuthApiClient _authClient;
    private readonly ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(ITokenAuthApiClient authClient, ILogger<AuthHeaderHandler> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authClient.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            if (request.Headers.Authorization == null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        else
        {
            _logger.LogDebug("No auth token present when sending request to {Url}", request.RequestUri);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
