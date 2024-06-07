namespace Lagerhotell.Services;
using System.Net.Http.Headers;

public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly SessionService _sessionService;
    private readonly string url = "https://localhost:7272/users";

    public UserService(HttpClient httpClient, SessionService sessionService)
    {
        _httpClient = httpClient;
        _sessionService = sessionService;
    }

    public async Task<User> GetUserByPhoneNumber(string phoneNumber)
    {
        string endpointUrl = url + "/get-user-by-phone-number/" + phoneNumber;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            User user = JsonSerializer.Deserialize<GetUserByPhoneNumber.GetUserByPhoneNumberResponse>(responseContent, options).User;
            return user;
        }
        else
        {
            throw new KeyNotFoundException($"Something went wrong when fetching user with phone number: {phoneNumber} ");
        }
    }

}
