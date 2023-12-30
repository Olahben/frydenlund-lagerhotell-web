using LagerhotellAPI.Models;
using System.Text;
using System.Text.Json;

namespace Lagerhotell.Services.UserService
{
    public class UserServiceUserSettings
    {
        private readonly string baseUrl = "https://localhost:7272/users";
        public async Task<User> LoadUser(HttpClient client, User user, string userId)
        {
            string url = baseUrl + "/get-user";
            var request = new LagerhotellAPI.Models.GetUserRequest { UserId = userId };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Console.WriteLine("Does this happen");
            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                LagerhotellAPI.Models.User deserializedUser = JsonSerializer.Deserialize<LagerhotellAPI.Models.User>(result, options);
                Console.WriteLine(deserializedUser);
                user.FirstName = deserializedUser.FirstName;
                user.LastName = deserializedUser.LastName;
                user.PhoneNumber = deserializedUser.PhoneNumber;
                user.BirthDate = deserializedUser.BirthDate;
                user.Password = deserializedUser.Password;
                return user;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return user;
            }
        }
    }
}
