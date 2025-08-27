using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

namespace Microsoft.eShopWeb.Web.Infrastructure.Http;

/// <summary>
/// Handles 401/403 responses from PublicApi. Clears stored token so next UI navigation forces login.
/// Optionally adds a header the UI could check for client-side redirect logic.
/// </summary>
public class Http401Handler : DelegatingHandler
{
    private readonly ITokenAuthApiClient _authClient;
    private readonly ILogger<Http401Handler> _logger;

    public Http401Handler(ITokenAuthApiClient authClient, ILogger<Http401Handler> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            _logger.LogInformation("Clearing auth token after {Status} from {Url}", response.StatusCode, request.RequestUri);
            await _authClient.LogoutAsync();
            // Signal to UI (could be used by JS) that auth expired
            if (!response.Headers.Contains("X-Auth-Expired"))
            {
                response.Headers.Add("X-Auth-Expired", "1");
            }
        }
        return response;
    }
}
