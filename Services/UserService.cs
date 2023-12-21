using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;


namespace Lagerhotell.Services
{
    public class UserService
    {
        private readonly HttpClient client;
        private readonly NavigationManager navigationManager;
        private readonly string BaseUrlAPI = "https://localhost:7272/users";
        public async Task<string> CheckPhoneNumber(string phoneNumber)
        {
            string url = BaseUrlAPI + "is-phone-number-registered-registration";
            var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            if (response.ReasonPhrase == "OK")
            {
                return "Dette mobilnummeret er ikke registrert i våre systemer";
            }
            else
            {
                return "";
            }
        }
    }
}
