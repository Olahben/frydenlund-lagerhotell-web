using LagerhotellAPI.Models.CustomExceptionModels;

namespace Lagerhotell.Services;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7272/auth";

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
