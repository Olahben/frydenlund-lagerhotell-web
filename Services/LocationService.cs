global using LagerhotellAPI.Models.DomainModels;
global using LagerhotellAPI.Models.FrontendModels;
global using System.Net;
global using System.Text;
global using System.Text.Json;

namespace Lagerhotell.Services;

public class LocationService
{
    private readonly HttpClient client = new HttpClient();
    private readonly string _baseUrl = "https://localhost:7272/locations";

    public async Task<bool> AddLocation(string name, bool active, string token)
    {
        Location location = new Location(name, active);
        AddLocationRequest request = new AddLocationRequest(location);
        string url = _baseUrl + "/add";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Lokasjonen eksisterer allerede");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Sessionen har gått ut");
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> DeleteLocation(string locationName, string token)
    {
        string url = _baseUrl + $"/{locationName}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.DeleteAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lokasjonen eksisterer ikke");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Sessionen har gått ut");
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> ChangeLocation(string oldLocationName, string newLocationName, bool active, string token)
    {
        Location location = new Location(newLocationName, active);
        ModifyLocationRequest request = new ModifyLocationRequest(oldLocationName, location);
        string url = _baseUrl + "/modify";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PutAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lokasjonen eksisterer ikke");
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Lokasjonen eksisterer allerede");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Sessionen har gått ut");
        }
        else
        {
            return false;
        }
    }
}
