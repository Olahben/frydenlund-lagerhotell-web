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

    public async Task<string> CreateCompanyUserAsync(CompanyUser companyUser)
    {
        string url = _baseUrl;
        string companyUserJson = JsonSerializer.Serialize(new CreateCompanyUserRequest(companyUser));
        StringContent content = new(companyUserJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            await _sessionService.AddJwtToLocalStorage(JsonSerializer.Deserialize<CreateCompanyUserResponse>(responseString).UserAcessToken);
            return JsonSerializer.Deserialize<CreateCompanyUserResponse>(responseString).CompanyUserId;
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
}
