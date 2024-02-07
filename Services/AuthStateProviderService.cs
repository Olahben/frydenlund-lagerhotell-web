using Lagerhotell.Services.UserService;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;


namespace Lagerhotell.Services
{
    public class AuthStateProviderService : AuthenticationStateProvider
    {
        private readonly SessionService _sessionService;
        private readonly HttpClient _tokenHttpClient;
        private readonly HttpClient _backofficeHttpClient;


        public AuthStateProviderService(SessionService sessionService, HttpClient tokenHttpClient, HttpClient backofficeHttpClient)
        {
            _tokenHttpClient = tokenHttpClient;
            _backofficeHttpClient = backofficeHttpClient;
            _sessionService = sessionService;
        }

        private IEnumerable<Claim> Parse(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer
                .Deserialize<Dictionary<string, object>>(jsonBytes);

            IEnumerable<Claim> claims = keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? accessToken = await _sessionService.GetJwtFromLocalStorage();

            var identity = new ClaimsIdentity();
            _tokenHttpClient.DefaultRequestHeaders.Authorization = null;
            _backofficeHttpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    identity = new ClaimsIdentity(Parse(accessToken), "jwt");
                    accessToken = accessToken.Replace("\"", "").Replace("\"", "");

                    _tokenHttpClient
                        .DefaultRequestHeaders
                        .Authorization = new AuthenticationHeaderValue("jwtToken", accessToken);

                    _backofficeHttpClient
                        .DefaultRequestHeaders
                        .Authorization = new AuthenticationHeaderValue("jwtToken", accessToken);
                }
                catch
                {
                    await _sessionService.RemoveItemAsync("jwtToken");
                    identity = new ClaimsIdentity();
                }
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }
    }
}
