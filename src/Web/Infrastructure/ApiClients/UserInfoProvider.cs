using System.Net.Http.Json;
using BlazorShared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

public interface IUserInfoProvider
{
    Task<string?> GetUserNameAsync();
    Task<IReadOnlyList<string>> GetRolesAsync();
}

public class UserInfoProvider : IUserInfoProvider
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAuthApiClient _auth;
    private readonly ILogger<UserInfoProvider> _logger;
    private UserInfoCache? _cache;

    public UserInfoProvider(HttpClient httpClient, ITokenAuthApiClient auth, IOptions<BaseUrlConfiguration> urls, ILogger<UserInfoProvider> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(urls.Value.ApiBase);
        _auth = auth;
        _logger = logger;
    }

    public async Task<string?> GetUserNameAsync()
    {
        await EnsureInfo();
        return _cache?.UserName;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        await EnsureInfo();
        return _cache?.Roles?.AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    private async Task EnsureInfo()
    {
        if (_cache != null && _cache.ExpiresAt > DateTimeOffset.UtcNow) return;
        if (!await _auth.IsAuthenticatedAsync()) { _cache = null; return; }
        try
        {
            var resp = await _httpClient.GetAsync("auth/me");
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info: {Code}", resp.StatusCode);
                return;
            }
            var data = await resp.Content.ReadFromJsonAsync<UserInfoResponse>();
            if (data != null)
            {
                _cache = new UserInfoCache
                {
                    UserName = data.UserName,
                    Roles = data.Roles ?? new List<string>(),
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info");
        }
    }

    private class UserInfoResponse { public string UserName { get; set; } = string.Empty; public List<string> Roles { get; set; } = new(); }
    private class UserInfoCache { public string UserName { get; set; } = string.Empty; public List<string> Roles { get; set; } = new(); public DateTimeOffset ExpiresAt { get; set; } }
}
