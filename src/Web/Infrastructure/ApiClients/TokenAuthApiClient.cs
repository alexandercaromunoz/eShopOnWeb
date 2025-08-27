using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BlazorShared;

namespace Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

public interface ITokenAuthApiClient
{
    Task<bool> LoginAsync(string userName, string password, bool rememberMe = false);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class TokenAuthApiClient : ITokenAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<TokenAuthApiClient> _logger;
    private const string TOKEN_KEY = "eshop_token";
    private const string EXP_KEY = "eshop_token_exp";

    public TokenAuthApiClient(HttpClient httpClient, ILocalStorageService localStorage, IOptions<BaseUrlConfiguration> urls, ILogger<TokenAuthApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(urls.Value.ApiBase);
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string userName, string password, bool rememberMe = false)
    {
        try
        {
            var payload = new { userName, password };
            var resp = await _httpClient.PostAsJsonAsync("auth/login", payload);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed for {User}", userName);
                return false;
            }
            var data = await resp.Content.ReadFromJsonAsync<LoginResult>() ?? new();
            await _localStorage.SetItemAsync(TOKEN_KEY, data.Token);
            await _localStorage.SetItemAsync(EXP_KEY, data.ExpiresAt);
            return true;
        }
        catch (InvalidOperationException)
        {
            // Prerender: local storage unavailable – defer auth
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(EXP_KEY);
        }
        catch (InvalidOperationException)
        {
            // Ignore during prerender
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
            if (string.IsNullOrWhiteSpace(token)) return null;
            var exp = await _localStorage.GetItemAsync<DateTimeOffset?>(EXP_KEY);
            if (exp.HasValue && exp.Value < DateTimeOffset.UtcNow)
            {
                await LogoutAsync();
                return null;
            }
            return token;
        }
        catch (InvalidOperationException)
        {
            // Prerender path – JS interop not yet available
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync() => (await GetTokenAsync()) != null;

    private class LoginResult
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
