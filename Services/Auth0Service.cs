using LagerhotellAPI.Models.CustomExceptionModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Lagerhotell.Services;

public class Auth0Service
{
    private readonly HttpClient client = new();
    private readonly string _auth0Domain;
    private readonly string _auth0FullDomain;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _loginCallBackUrl;
    private readonly string _apiClientId;
    private readonly SessionService _sessionService;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly string _baseUrl = "https://localhost:7272/auth0-users";

    public Auth0Service(IConfiguration configuration, SessionService sessionService, NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider)
    {
        _auth0Domain = configuration["Auth0:Domain"];
        _auth0FullDomain = $"https://{_auth0Domain}";
        _clientId = configuration["Auth0:ClientId"];
        _clientSecret = configuration["Auth0:ClientSecret"];
        _loginCallBackUrl = $"https://localhost:5001{configuration["Auth0:LoginCallback"]}";
        _apiClientId = configuration["Auth0:ApiClientId"];
        _sessionService = sessionService;
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public static string GenerateState()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    // Validates state in login callback to check if it is the same as the one generated here
    public async Task RedirectToLoginPage()
    {
        string state = GenerateState();
        await _sessionService.AddLoginStateToLocalStorage(state);
        string redirectUrl = $"https://{_auth0Domain}/authorize?response_type=code&client_id={_apiClientId}&redirect_uri={_loginCallBackUrl}&state={state}&scope=openid profile email offline_access";
        _navigationManager.NavigateTo(redirectUrl);
    }

    public async Task ExchangeCodeForTokenAndLogin(string code)
    {
        string endpoint = _baseUrl + "/exchange-code-for-tokens";
        ExchangeCodeForTokensRequest request = new(code);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonSerializer.Deserialize<ExchangeCodeForTokensResponse>(responseContent);
        await _sessionService.AddJwtToLocalStorage(deserializedResponse.AccessToken);
        _authenticationStateProvider.AuthenticationStateChanged += async (Task<AuthenticationState> task) =>
        {
            var authState = await task;
            var user = authState.User;
        };
        _navigationManager.NavigateTo($"user/{deserializedResponse.UserId}");
    }
}
