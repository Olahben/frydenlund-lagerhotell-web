using Lagerhotell.Pages.Signup;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lagerhotell.Services;

public class UserServiceSignup
{
    private readonly string _baseUrl = "https://localhost:7272/users";
    protected string? CustomError;
    protected bool UserRegistered;
    private readonly HttpClient _httpClient = new();
    public async Task<(string userId, string token)> AddUser(string firstName, string lastName, string phoneNumber, string birthDate, string address, string postalCode, string city, string password, bool IsAdministrator, HttpClient client, string email)
    {
        var request = new AddUserRequest { FirstName = firstName, LastName = lastName, PhoneNumber = phoneNumber, BirthDate = birthDate, Address = address, PostalCode = postalCode, City = city, Password = password, IsAdministrator = IsAdministrator, Email = email };
        string url = _baseUrl + "/add-user";
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
            return (userIdResponse.UserId, userIdResponse.Token);
        }
        throw new Exception("Brukeren er allerede registrert");
    }
    public async Task? PhoneNumberExistence(string phoneNumber, HttpClient client)
    {
        string url = _baseUrl + "/check-phone/" + phoneNumber;
        HttpResponseMessage response = await client.GetAsync(url);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        CheckPhoneNumber.CheckPhoneNumberResponse deserializedResponse = JsonSerializer.Deserialize<CheckPhoneNumber.CheckPhoneNumberResponse>(await response.Content.ReadAsStringAsync(), options);
        if (deserializedResponse.PhoneNumberExistence)
        {
            throw new InvalidOperationException("Brukeren er allerede registrert");
        }
        CustomError = "";
        UserRegistered = false;
    }

    /// <summary>
    /// Method that signs up the user and takes a SignupForm.AccountFormValues as parameter
    /// </summary>
    /// <param name="accountFormValues"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<(string userId, string token)> SignupUser(SignupForm.AccountFormValues accountFormValues, HttpClient client)
    {
        string userId;
        string token;
        try
        {
            await PhoneNumberExistence(accountFormValues.PhoneNumber, client);
            (userId, token) = await AddUser(accountFormValues.FirstName, accountFormValues.FirstName, accountFormValues.PhoneNumber, accountFormValues.BirthDate, accountFormValues.Address, accountFormValues.PostalCode, accountFormValues.City, accountFormValues.Password, accountFormValues.IsAdministrator, client, accountFormValues.Email);
            return (userId, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("In SignupUser in SignupService" + ex);
            throw new Exception("Brukeren er allerede registrert");
        }

    }

    /// <summary>
    /// Method that signs up the user and takes a User model as parameter
    /// </summary>
    /// <param name="user"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<(string userId, string token)> SignupUserWithUserModel(User user)
    {
        string userId;
        string token;
        try
        {
            await PhoneNumberExistence(user.PhoneNumber, _httpClient);
            (userId, token) = await AddUser(user.FirstName, user.FirstName, user.PhoneNumber, user.BirthDate, user.Address.StreetAddress, user.Address.PostalCode, user.Address.City, user.Password, user.IsAdministrator, _httpClient, user.Email);
            return (userId, token);
        }
        catch (Exception)
        {
            throw new Exception("Brukeren er allerede registrert");
        }

    }
    public class ContainsNumberAttribute : ValidationAttribute
    {
        // Overrides the existing method IsValid in the ValidationAttribute class
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? password = value as string;
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
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? phoneNumber = value as string;
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
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? birthDateStr = value as string;
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

    public class ContainsNumbersAndLettersAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? addressStr = value as string;
            bool containsInt = addressStr.Any(char.IsDigit);
            bool containsLetter = addressStr.Any(char.IsLetter);
            if (containsInt && containsLetter)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Dette feltet må inneholde både bokstaver og tall");
            }

        }
    }
    public class ContainsOnlyLettersAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? valueStr = value as string;
            if (Regex.IsMatch(valueStr, @"^[a-zA-Z]+$"))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Dette feltet kan bare inneholde bokstaver");
            }
        }
    }
}
