using Lagerhotell.Pages.Signup;
using LagerhotellAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Lagerhotell.Services.UserService
{
    public class UserServiceSignup
    {
        // private static readonly HttpClient client = new();
        // public Signup signup = new();
        private readonly string _baseUrl = "https://localhost:7272/users/";
        protected string? CustomError;
        protected bool UserRegistered;
        public async Task<string> AddUser(string firstName, string lastName, string phoneNumber, string birthDate, string password, HttpClient client)
        {
            var request = LagerhotellAPI.Models.AddUserRequest.AddUserRequestFunc(firstName, lastName, phoneNumber, birthDate, password);
            string url = _baseUrl + "add-user";
            string jsonData = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string responseContent = await response.Content.ReadAsStringAsync();
                AddUserResponse userIdResponse = JsonSerializer.Deserialize<AddUserResponse>(responseContent, options);
                return userIdResponse.UserId;
            }
            throw new Exception("Brukeren er allerede registrert");
        }
        public async Task? PhoneNumberExistence(string phoneNumber, HttpClient client)
        {
            string url = _baseUrl + "is-phone-number-registered-registration";
            var request = new LagerhotellAPI.Models.CheckPhoneNumber.CheckPhoneNumberRequest { PhoneNumber = phoneNumber };
            string jsonData = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                CustomError = "";
                UserRegistered = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> SignupUser(SignupForm.AccountFormValues accountFormValues, HttpClient client)
        {
            try
            {
                await PhoneNumberExistence(accountFormValues.PhoneNumber, client);
                return await AddUser(accountFormValues.FirstName, accountFormValues.FirstName, accountFormValues.PhoneNumber, accountFormValues.BirthDate, accountFormValues.Password, client);
            }
            catch (Exception ex)
            {
                throw new Exception("Brukeren er allerede registrert");
            }

        }
        public class ContainsNumberAttribute : ValidationAttribute
        {
            // Overrides the existing method IsValid in the ValidationAttribute class
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                string password = value as string;
                // Checks if the password contains a number
                if (password.Any(char.IsDigit))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Passordet ditt må ha minst ett tall");
                }
            }
        }

        public class ContainsOnlyNumbersAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                string phoneNumber = value as string;
                if (int.TryParse(phoneNumber, out int result))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Telefonnummeret ditt kan bare inneholde tall");
                }
            }
        }
        // Inherits from the ValidationAttribute class
        public class IsOverLegalAgeAttribute : ValidationAttribute
        {
            // Overrides the existing method IsValid in the ValidationAttribute class
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                string birthDateStr = value as string;
                // format is with hyphens
                DateTime birthDate = DateTime.ParseExact(birthDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                int age = DateTime.Now.Year - birthDate.Year;
                // Checks exactly if the person is over 18
                if (DateTime.Now.Month < birthDate.Month || (DateTime.Now.Month == birthDate.Month && DateTime.Now.Day < birthDate.Day))
                    age--;

                if (age >= 18)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Du må være over 18 år");
                }

            }
        }
    }
}
