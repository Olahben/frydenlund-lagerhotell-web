using LagerhotellAPI.Models.CustomExceptionModels;
using LagerhotellAPI.Models.DbModels.Auth0;
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
    private readonly string _customApiUrl;
    private readonly SessionService _sessionService;
    private readonly NavigationManager _navigationManager;
    private readonly UserService _userService;
    private readonly CompanyUserService _companyUserService;
    private readonly AppState _appState;
    private readonly string _baseUrl = "https://localhost:7272/auth0-users";

    public Auth0Service(IConfiguration configuration, SessionService sessionService, NavigationManager navigationManager, UserService userService, CompanyUserService companyUserService, AppState appState)
    {
        _auth0Domain = configuration["Auth0:Domain"];
        _auth0FullDomain = $"https://{_auth0Domain}";
        _clientId = configuration["Auth0:ClientId"];
        _clientSecret = configuration["Auth0:ClientSecret"];
        _loginCallBackUrl = $"https://localhost:5001{configuration["Auth0:LoginCallback"]}";
        _apiClientId = configuration["Auth0:ApiClientId"];
        _customApiUrl = configuration["Auth0:ApiUrl"];
        _sessionService = sessionService;
        _navigationManager = navigationManager;
        _userService = userService;
        _companyUserService = companyUserService;
        _appState = appState;
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
        string redirectUrl = $"https://{_auth0Domain}/authorize?response_type=code&client_id={_apiClientId}&redirect_uri={_loginCallBackUrl}&state={state}&scope=openid profile email offline_access&audience={_customApiUrl}&ui_locales=nb";
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
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var deserializedResponse = JsonSerializer.Deserialize<ExchangeCodeForTokensResponse>(responseContent, options);
        await _sessionService.AddJwtToLocalStorage(deserializedResponse.AccessToken);
        _navigationManager.NavigateTo($"user/{deserializedResponse.UserId}");
    }

    public async Task<string> RefreshAccessToken(string auth0Id)
    {
        string endpoint = _baseUrl + "/refresh-token";
        var request = new RefreshAccessTokenRequest(auth0Id);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonSerializer.Deserialize<RefreshAccessTokenResponse>(responseContent);
        return deserializedResponse.AccessToken;
    }

    public async Task<UserAuth0> GetAuth0UserbyAuth0Id(string auth0Id)
    {
        string endpoint = _baseUrl + $"/get-user/{auth0Id}";
        var response = await client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var deserializedResponse = JsonSerializer.Deserialize<GetAuth0UserResponse>(responseContent, options);
        return deserializedResponse.User;
    }

    public async Task SendNewVerificationEmail(string auth0Id)
    {
        string endpoint = _baseUrl + "/send-verification-email";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        var request = new SendVerificationEmailRequest(auth0Id);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelEmailVerification(string userId)
    {
        // The methods that delete the user in the database also delete the user in Auth0
        try
        {
            var user = await _userService.GetUserById(userId);
            if (user.IsEmailVerified)
            {
                throw new InvalidOperationException("User has already verified their email");
            }
            await _userService.DeleteUser(userId);
            await _sessionService.LogUserOut();
            _navigationManager.NavigateTo("/registrer-deg");
        }
        catch (KeyNotFoundException)
        {
            var user = await _companyUserService.GetCompanyUserAsync(userId);
            if (user.IsEmailVerified)
            {
                throw new InvalidOperationException("User has already verified their email");
            }
            await _companyUserService.DeleteCompanyUserAsync(userId);
            await _sessionService.LogUserOut();
            _navigationManager.NavigateTo("/registrer-deg");
        }
    }

    public async Task SendForgotPasswordEmail(string email)
    {
        string endpoint = _baseUrl + "/send-forgot-password-email";
        var request = new SendForgotPasswordEmailRequest(email);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdatePassword(string auth0Id, string email, string oldPassword, string newPassword)
    {
        string endpoint = _baseUrl + "/change-user-password";
        var request = new ChangeUserPasswordRequest(auth0Id, email, oldPassword, newPassword);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PatchAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUser(string auth0Id)
    {
        string endpoint = _baseUrl + $"/delete-user";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new Auth0IdRequest(auth0Id)), Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await _sessionService.LogUserOut();
        _appState.UserLoggedOut();
        _navigationManager.NavigateTo("/registrer-deg");
    }
}
