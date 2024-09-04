using LagerhotellAPI.Models.ValueTypes;
using static Lagerhotell.Pages.UserSettings.UserSettings;

namespace Lagerhotell.Services;

public class UserServiceUserSettings
{
    private readonly string _baseUrl = "https://localhost:7272/users";
    public async Task<UserValues> LoadUser(HttpClient client, UserValues user, string userId, string jwtToken)
    {
        string url = _baseUrl + "/get-user/" + userId;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            User deserializedUser = JsonSerializer.Deserialize<GetUser.GetUserResponse>(result, options).User;
            UserValues userValues = new UserValues { Id = deserializedUser.Id, FirstName = deserializedUser.FirstName, LastName = deserializedUser.LastName, PhoneNumber = deserializedUser.PhoneNumber, BirthDate = deserializedUser.BirthDate, Password = deserializedUser.Password, StreetAddress = deserializedUser.Address.StreetAddress, PostalCode = deserializedUser.Address.PostalCode, City = deserializedUser.Address.City, IsAdministrator = deserializedUser.IsAdministrator, Email = deserializedUser.Email };
            return userValues;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("User not found");
        }
        else
        {
            throw new Exception("Failed to get user");
        }
    }

    public async Task<bool>? UpdateUserValues(HttpClient httpClient, UserValues userValues, string jwtToken)
    {
        string url = _baseUrl + "/update-user-values";
        Address addressRequest = new(userValues.StreetAddress, userValues.PostalCode, userValues.City);
        UpdateUserValuesRequest request = new UpdateUserValuesRequest { FirstName = userValues.FirstName, LastName = userValues.LastName, PhoneNumber = userValues.PhoneNumber, BirthDate = userValues.BirthDate, Address = addressRequest, Password = userValues.Password, IsAdministrator = userValues.IsAdministrator, Email = userValues.Email };
        string jsonData = JsonSerializer.Serialize(request);
        StringContent stringContentRequest = new StringContent(jsonData, Encoding.UTF8, "application/json");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        HttpResponseMessage response = await httpClient.PutAsync(url, stringContentRequest);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            throw new Exception("Brukeren eksisterer ikke");
        }
    }
}
