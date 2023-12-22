using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;


namespace Lagerhotell.Services
{
    public class UserService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly NavigationManager navigationManager;
        private readonly string BaseUrlAPI = "https://localhost:7272/users";
        public class UserServiceLogin
        {
            private readonly UserService userService = new UserService();
            public string PhoneNumberRegistered = "";
            public string CustomError = "";
            public string PasswordError = "";

            public async Task<string> CheckPhoneNumber(string phoneNumber)
            {
                string url = userService.BaseUrlAPI + "/is-phone-number-registered-registration";
                Console.WriteLine(url);
                var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
                string jsonData = JsonSerializer.Serialize(request);
                StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await userService.client.PostAsync(url, stringContent);
                if (response.ReasonPhrase == "OK")
                {
                    return "Dette mobilnummeret er ikke registrert i våre systemer";
                }
                else
                {
                    return "";
                }
            }
            public async Task<string> CheckPassword(string password, string phoneNumber)
            {
                string url = userService.BaseUrlAPI + "/users/check-password";
                var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
                string jsonData = JsonSerializer.Serialize(request);
                StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await userService.client.PostAsync(url, stringContent);
                // Should handle the response, compare the passwords and return an error if the passwords do not match
                Console.WriteLine("Response:" + response.Content.ReadAsStringAsync().Result.ToString());
                if (response.Content.ReadAsStringAsync().Result.ToString().Contains(password))
                {
                    // Sucessfull login
                    return "";
                }
                else
                {
                    return "Feil Passord";
                }
            }

            public async Task? GetUserByPhoneNumber(string phoneNumber)
            {
                string url = "https://localhost:7272/users/get-user-by-phone-number";
                var request = new LagerhotellAPI.Models.GetUserByPhoneNumberRequest { PhoneNumber = phoneNumber };
                string jsonData = JsonSerializer.Serialize(request);
                StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await userService.client.PostAsync(url, stringContent);
            }
            public async Task RedirectToUserSettings(string id, NavigationManager navigationManager)
            {
                navigationManager.NavigateTo($"/user/{id}");
            }

            public async Task? LoginUser(string phoneNumber, string password)
            {
                await CheckPhoneNumber(phoneNumber);
                if (CustomError == "")
                {
                    await CheckPassword(password, phoneNumber);
                }
                if (PasswordError == "" && CustomError == "")
                {
                    // Redirect to user/UserId, call API controller
                    await GetUserByPhoneNumber(phoneNumber);
                    // Use what returned by GetUserByPhoneNumber, the id To Call RedirectToUserSettings
                    // RedirectToUserSettings()
                    // Generate Session
                }
            }
        }
    }
}
