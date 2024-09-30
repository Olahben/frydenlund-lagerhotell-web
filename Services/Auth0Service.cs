using Microsoft.AspNetCore.Components;
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
    private readonly SessionService _sessionService;
    private readonly NavigationManager _navigationManager;

    public Auth0Service(IConfiguration configuration, SessionService sessionService, NavigationManager navigationManager)
    {
        _auth0Domain = configuration["Auth0:Domain"];
        _auth0FullDomain = $"https://{_auth0Domain}";
        _clientId = configuration["Auth0:ClientId"];
        _clientSecret = configuration["Auth0:ClientSecret"];
        _loginCallBackUrl = $"https://localhost:5001{configuration["Auth0:LoginCallback"]}";
        _sessionService = sessionService;
        _navigationManager = navigationManager;
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
        string redirectUrl = $"https://{_auth0Domain}/authorize?response_type=code&client_id={_clientId}&redirect_uri={_loginCallBackUrl}&state={state}";
        _navigationManager.NavigateTo(redirectUrl);
    }
}
