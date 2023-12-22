using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;


namespace Lagerhotell.Services.UserService
{
    public class UserServiceLogin
    {
        private readonly HttpClient client = new HttpClient();
        private readonly NavigationManager navigationManager;
        private readonly string BaseUrlAPI = "https://localhost:7272/users";
        // Remove
        public string PhoneNumberRegistered = "";
        public string CustomError = "";
        public string PasswordError = "";

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
            // Should handle the response, compare the passwords and return an error if the passwords do not match
            Console.WriteLine("Response:" + response.Content.ReadAsStringAsync().Result.ToString());
            return response.Content.ReadAsStringAsync().Result.ToString().Contains(password);
        }

        public async Task? GetUserByPhoneNumber(string phoneNumber)
        {
            string url = "https://localhost:7272/users/get-user-by-phone-number";
            var request = new LagerhotellAPI.Models.GetUserByPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
        }
        public async Task RedirectToUserSettings(string id, NavigationManager navigationManager)
        {
            navigationManager.NavigateTo($"/user/{id}");
        }

        public async Task<string>? LoginUser(string phoneNumber, string password)
        {
            // await CheckPhoneNumber(phoneNumber);
            bool userExistence = await CheckPhoneNumber(phoneNumber);
            if (!userExistence)
            {
                throw new Exception("Brukeren eksisterer ikke");
            }

            bool passwordCorrectness = await CheckPassword(password, phoneNumber);
            if (!passwordCorrectness)
            {
                throw new Exception("Feil passord");
            }
            // Redirect to user/UserId, call API controller
            await GetUserByPhoneNumber(phoneNumber);
            // Use what returned by GetUserByPhoneNumber, the id To Call RedirectToUserSettings
            // RedirectToUserSettings()
            // Generate Session
            return "";

        }
    }
}
