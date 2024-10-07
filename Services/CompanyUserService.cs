using System.Data.SqlTypes;

namespace Lagerhotell.Services;

public class CompanyUserService
{
    private readonly HttpClient client = new();
    private readonly string _baseUrl = "https://localhost:7272/company-users";
    private readonly SessionService _sessionService;

    public CompanyUserService(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<CompanyUser> GetCompanyUserAsync(string companyUserId)
    {
        string url = _baseUrl + $"/{companyUserId}";
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {

            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            CompanyUser companyUser = JsonSerializer.Deserialize<GetCompanyUserResponse>(responseString, options).CompanyUser;
            return companyUser;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Company user not found");
        }
        else
        {
            throw new Exception("Failed to get company user");
        }
    }

    public async Task<List<CompanyUser>> GetCompanyUsersAsync(int? skip, int? take)
    {
        string url = _baseUrl + $"/all/{skip}/{take}";
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<CompanyUser> companyUsers = JsonSerializer.Deserialize<GetCompanyUsersResponse>(responseString, options).CompanyUsers;
            return companyUsers;
        }
        else
        {
            throw new Exception("Failed to get company users");
        }
    }

    public async Task<CompanyUser> GetCompanyUserByPhoneNumber(string phoneNumber)
    {
        string url = _baseUrl + $"/get-by-phone-number/{phoneNumber}";
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            CompanyUser companyUser = JsonSerializer.Deserialize<GetCompanyUserResponse>(responseString, options).CompanyUser;
            return companyUser;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Company user not found");
        }
        else
        {
            throw new Exception("Failed to get company user");
        }
    }

    public async Task<string> CreateCompanyUserAsync(CompanyUser companyUser)
    {
        string url = _baseUrl + "/create";

        // Populate to escape bad request
        companyUser.CompanyUserId = "";
        companyUser.Auth0Id = "";

        string companyUserJson = JsonSerializer.Serialize(new CreateCompanyUserRequest(companyUser));
        StringContent content = new(companyUserJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            await _sessionService.AddJwtToLocalStorage(JsonSerializer.Deserialize<CreateCompanyUserResponse>(responseString, options).UserAcessToken);
            return JsonSerializer.Deserialize<CreateCompanyUserResponse>(responseString, options).CompanyUserId;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Company not found");
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new SqlAlreadyFilledException("Company user already exists");
        }
        else
        {
            throw new Exception("Failed to create company user");
        }
    }

    public async Task UpdateCompanyUserAsync(CompanyUser companyUser)
    {
        string url = _baseUrl + "/modify";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        string companyUserJson = JsonSerializer.Serialize(new UpdateCompanyUserRequest(companyUser));
        StringContent content = new(companyUserJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Company user not found");
            }
            else
            {
                throw new Exception("Failed to update company user");
            }
        }
    }

    public async Task DeleteCompanyUserAsync(string companyUserId)
    {
        string url = _baseUrl + $"/{companyUserId}";
        HttpResponseMessage response = await client.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Company user not found");
            }
            else
            {
                throw new Exception("Failed to delete company user");
            }
        }
    }

    public async Task<string> LoginByEmail(CompanyUser companyUser)
    {
        LoginCompanyUserByEmailRequest request = new(companyUser.Email, companyUser.Password);
        string url = _baseUrl + "/login";
        string requestJson = JsonSerializer.Serialize(request);
        StringContent content = new(requestJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            await _sessionService.AddJwtToLocalStorage(JsonSerializer.Deserialize<LoginCompanyUserByEmailResponse>(responseString, options).AccessToken);
            return JsonSerializer.Deserialize<LoginCompanyUserByEmailResponse>(responseString, options).UserId;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Company user not found");
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new SqlTypeException("Wrong password1");
        }
        else
        {
            throw new Exception("Failed to login company user");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newPassword"></param>
    /// <param name="oldPassword"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="SqlAlreadyFilledException"></exception>
    /// <exception cref="SqlTypeException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task ResetPassword(string id, string newPassword, string oldPassword)
    {
        var request = new ResetPasswordRequest(id, newPassword, oldPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        string url = _baseUrl + "/reset-password";
        string requestJson = JsonSerializer.Serialize(request);
        StringContent content = new(requestJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Company user not found");
            }
            else if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                throw new SqlAlreadyFilledException("Password already used");
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new SqlTypeException("Wrong password");
            }
            else
            {
                throw new Exception($"Failed to reset password, code: {response.StatusCode}");
            }
        }
    }

    public async Task<List<CompanyUser>> GetAll(int? skip, int? take)
    {
        string url = _baseUrl + $"/all";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<CompanyUser> companyUsers = JsonSerializer.Deserialize<GetCompanyUsersResponse>(responseString, options).CompanyUsers;
            return companyUsers;
        }
        else
        {
            throw new Exception("Failed to get company users");
        }
    }

    public async Task<bool> DoesSimilarUserExist(string companyNumber, string phoneNumber, string email)
    {
        string url = _baseUrl + $"/does-similar-user-exist/{companyNumber}/{phoneNumber}/{email}";
        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Company not found in Norway");
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new SqlAlreadyFilledException("User already exists");
            }
            else
            {
                throw new Exception($"Failed to check if similar user exists, status code: {response.StatusCode}");
            }
        }
        return false;
    }

    public async Task<CompanyUser> GetCompanyUserByAuth0Id(string id)
    {
        string endpointUrl = _baseUrl + $"/get-user-by-auth0-id/{id}";
        HttpResponseMessage response = await client.GetAsync(endpointUrl);
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<GetCompanyUserResponse>(responseContent, options).CompanyUser;
    }
}
