using LagerhotellAPI.Models.DomainModels;
using LagerhotellAPI.Models.FrontendModels;
using System.Text;
using System.Text.Json;

namespace Lagerhotell.Services.UserService
{
    public class UserServiceLogin
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string _baseUrl = "https://localhost:7272/users";

        public async Task<bool> CheckPhoneNumber(string phoneNumber)
        {
            string url = _baseUrl + "/check-phone/" + phoneNumber;

            HttpResponseMessage response = await client.GetAsync(url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            CheckPhoneNumber.CheckPhoneNumberResponse deserializedResponse = JsonSerializer.Deserialize<CheckPhoneNumber.CheckPhoneNumberResponse>(await response.Content.ReadAsStringAsync(), options);
            return deserializedResponse.PhoneNumberExistence;
        }
        public async Task<(string, bool)> CheckPassword(string password, string phoneNumber)
        {
            string url = _baseUrl + "/log-in";
            var request = new Login.LoginRequest { PhoneNumber = phoneNumber, Password = password };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                // Read the string content of the response
                var responseString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                Jwt jwt = JsonSerializer.Deserialize<Jwt>(responseString, options);
                return (jwt.Token, true);
            }
            else
            {
                // Handle the error or throw an exception as appropriate
                throw new HttpRequestException($"Invalid response: {response.StatusCode}");
            }
        }

        public async Task<string> GetUserByPhoneNumber(string phoneNumber, string jwtToken)
        {
            string url = _baseUrl + "/get-user-by-phone-number/" + phoneNumber;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            HttpResponseMessage response = await client.GetAsync(url);
            // return User
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            User userDeserialized = JsonSerializer.Deserialize<User>(responseContent, options);
            string userId = userDeserialized.Id;
            return userId;
        }

        public async Task<(string Token, string UserId)> LoginUser(string phoneNumber, string password)
        {
            bool userExistence = await CheckPhoneNumber(phoneNumber);
            if (!userExistence)
            {
                throw new Exception("Brukeren eksisterer ikke");
            }
            (string token, bool passwordMatch) = await CheckPassword(password, phoneNumber);
            if (!passwordMatch)
            {
                throw new Exception("Feil passord");
            }
            string userId = await GetUserByPhoneNumber(phoneNumber, token);
            return (token, userId);
        }
    }
}
