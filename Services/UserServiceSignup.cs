using System.Text;
using System.Text.Json;

namespace Lagerhotell.Services.UserService
{
    public class UserServiceSignup
    {
        private static readonly HttpClient client = new HttpClient();
        public string PhoneNumberRegistered = "";
        public string CustomError = "";
        public async Task AddUser(string firstName, string lastName, string phoneNumber, string birthDate, string password)
        {
            var request = LagerhotellAPI.Models.AddUserRequest.AddUserRequestFunc(firstName, lastName, phoneNumber, birthDate, password);
            string url = "https://localhost:7272/users/adduser";
            string jsonData = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            // Handle the ID that is returned
        }
    }
}
