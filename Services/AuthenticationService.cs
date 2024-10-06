using LagerhotellAPI.Models.CustomExceptionModels;

namespace Lagerhotell.Services;

public class AuthenticateUserService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _baseUrl = "https://localhost:7272/auth";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task StartEmailVerification(string email)
    {
        var request = new StartEmailVerificationRequest(email);
        var json = JsonSerializer.Serialize(request);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl + "/start-email-verification", data);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to start email verification");
        }
    }

    /// <summary>
    /// Checks if the email verification code the user provided is valid
    /// </summary>
    /// <param name="email"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task VerifyEmailVerificationCode(string email, int code)
    {
        VerifyEmailVerificationCodeRequest request = new(code, email);
        var json = JsonSerializer.Serialize(request);
        StringContent data = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl + "/verify-email-verification-code", data);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new BadRequestException("Invalid verification code");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("No verification codes with this email not found");
            }
            else if (response.StatusCode == HttpStatusCode.Gone)
            {
                throw new InvalidOperationException("Verification code has expired");
            }
            else
            {
                throw new Exception("Failed to verify email verification code");
            }

        }
    }
}
