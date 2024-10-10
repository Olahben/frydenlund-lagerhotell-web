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

    public async Task<User> GetUserById(string id)
    {
        string endpointUrl = url + "/get-user/" + id;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            User user = JsonSerializer.Deserialize<GetUser.GetUserResponse>(responseContent, options).User;
            return user;
        }
        else
        {
            throw new KeyNotFoundException($"Something went wrong when fetching user with id: {id} ");
        }
    }
    /// <summary>
    /// Gets a user via their auth0Id
    /// Propagates the exception if the user is not found
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User> GetUserByAuth0Id(string id)
    {
        string endpointUrl = url + "/get-user-by-auth0-id/" + id;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        User user = JsonSerializer.Deserialize<GetUser.GetUserResponse>(responseContent, options).User;
        return user;
    }
        public async Task<List<User>> GetAllUsers(int? skip, int? take)
    {
        string endpointUrl = url + $"/get-all/{skip}/{take}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<User> users = JsonSerializer.Deserialize<List<User>>(responseContent, options);
            return users;
        }
        else
        {
            throw new InvalidOperationException($"Something went wrong when fetching users, code: {response.StatusCode}");
        }
    }

    public async Task DeleteUser(string id)
    {
        string endpointUrl = url + $"/delete-user/{id}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.DeleteAsync(endpointUrl);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("User not found");
            }
            else
            {
                throw new InvalidOperationException("Something went wrong when deleting user");
            }
        }
    }

    public async Task<bool> DoesSimilarUserExist(string phoneNumber, string email)
    {
        string endpointUrl = url + $"/does-similar-user-exist/{phoneNumber}/{email}";
        var response = await _httpClient.GetAsync(endpointUrl);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return true;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException();
            }
        }
        return false;
    }

    public async Task UpdateUser(User user)
    {
        string endpointUrl = url + "/update-user-values";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        var request = new UpdateUserValuesRequest2
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user.BirthDate,
            Address = user.Address,
            Password = user.Password,
            IsAdministrator = user.IsAdministrator,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified
        };
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpointUrl, data);
        response.EnsureSuccessStatusCode();
    }
}
