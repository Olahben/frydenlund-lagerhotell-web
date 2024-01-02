using LagerhotellAPI.Models;
using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;


namespace Lagerhotell.Services.UserService
{
    public class UserServiceLogin
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string BaseUrlAPI = "https://localhost:7272/users";

        public async Task<bool> CheckPhoneNumber(string phoneNumber)
        {
            string url = BaseUrlAPI + "/is-phone-number-registered-registration";
            var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> CheckPassword(string password, string phoneNumber)
        {
            string url = BaseUrlAPI + "/check-password";
            var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            return response.Content.ReadAsStringAsync().Result.ToString().Contains(password);
        }

        public async Task<string> GetUserByPhoneNumber(string phoneNumber)
        {
            string url = "https://localhost:7272/users/get-user-by-phone-number";
            var request = new LagerhotellAPI.Models.GetUserByPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            // return User
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            LagerhotellAPI.Models.User userDeserialized = JsonSerializer.Deserialize<LagerhotellAPI.Models.User>(responseContent, options);
            string userId = userDeserialized.Id;
            return userId;
        }
        public async Task? RedirectToUserSettings(string id, NavigationManager navigationManager)
        {
            navigationManager.NavigateTo($"/user/{id}");
        }

        public async Task<string> LoginUser(string phoneNumber, string password, NavigationManager navigationManager)
        {
            // await CheckPhoneNumber(phoneNumber);
            bool userExistence = await CheckPhoneNumber(phoneNumber);
            if (!userExistence)
            {
                return "Brukeren eksisterer ikke";
                throw new Exception("Brukeren eksisterer ikke");
            }

            bool passwordCorrectness = await CheckPassword(password, phoneNumber);
            if (!passwordCorrectness)
            {
                return "Feil passord";
                throw new Exception("Feil passord");
            }
            Console.WriteLine("phoneNumber" + phoneNumber);
            string userId = await GetUserByPhoneNumber(phoneNumber);
            await RedirectToUserSettings(userId, navigationManager);
            // Generate Session
            return "";

        }

        public async Task<CreateJwt.CreateJwtResponse> CreateJWT(CreateJwt.CreateJwtRequestService request)
        {
            string url = BaseUrlAPI + "/create-jwt";
            string jsonData = JsonSerializer.Serialize(request);
            Console.WriteLine("Json data" + jsonData);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            CreateJwt.CreateJwtResponse deserializedResponse = JsonSerializer.Deserialize<CreateJwt.CreateJwtResponse>(responseContent, options);
            Console.WriteLine(deserializedResponse.JWT);
            return deserializedResponse;
        }
    }
}
