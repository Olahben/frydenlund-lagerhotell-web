using LagerhotellAPI.Models;
using System.Text;
using System.Text.Json;
using static Lagerhotell.Pages.UserSettings.UserSettings;

namespace Lagerhotell.Services.UserService
{
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
                UserValues deserializedUser = JsonSerializer.Deserialize<UserValues>(result, options);
                Console.WriteLine(deserializedUser);
                user.FirstName = deserializedUser.FirstName;
                user.LastName = deserializedUser.LastName;
                user.PhoneNumber = deserializedUser.PhoneNumber;
                user.BirthDate = deserializedUser.BirthDate;
                user.Address = deserializedUser.Address;
                user.PostalCode = deserializedUser.PostalCode;
                user.City = deserializedUser.City;
                user.Password = deserializedUser.Password;
                return user;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return user;
            }
        }

        public async Task<bool>? UpdateUserValues(HttpClient httpClient, UserValues userValues, string jwtToken)
        {
            string url = _baseUrl + "/update-user-values";
            UpdateUserValuesRequest request = new UpdateUserValuesRequest { FirstName = userValues.FirstName, LastName = userValues.LastName, PhoneNumber = userValues.PhoneNumber, BirthDate = userValues.BirthDate, Address = userValues.Address, PostalCode = userValues.PostalCode, City = userValues.City, Password = userValues.Password };
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
}
