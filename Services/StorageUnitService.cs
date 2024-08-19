using LagerhotellAPI.Models.CustomExceptionModels;

namespace Lagerhotell.Services;

public class StorageUnitService
{
    private readonly HttpClient client = new HttpClient();
    private readonly string _baseUrl = "https://localhost:7272/storage-units";
    private readonly SessionService _sessionService;

    public StorageUnitService(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Adds a storage unit to the database
    /// <para>The endpoint that is called can only be called by administrators</para>
    /// </summary>
    /// <param name="storageUnitRequest"></param>
    /// <returns>Storage unit Id</returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> AddStorageUnit(StorageUnit storageUnitRequest, string linkedWarehouseHotelName, string token)
    {
        string url = _baseUrl + "/add";
        CreateStorageUnitRequest request = new CreateStorageUnitRequest { StorageUnit = storageUnitRequest, LinkedWarehouseHotelName = linkedWarehouseHotelName };
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string deserializedResponseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserializedResponse = JsonSerializer.Deserialize<CreateStorageUnitResponse>(deserializedResponseString, options);
            return deserializedResponse.StorageUnitId;
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException("Noe gikk galt");
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Lagerenheten eksisterer allerede");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    /// <summary>
    /// Gets the linked storage units to the warehouse hotel with the given Id
    /// </summary>
    /// <param name="warehouseHotelId"></param>
    /// <returns>A list of the storage units</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<List<StorageUnit>> GetRelevantStorageUnits(string warehouseHotelId)
    {
        var response = await client.GetAsync($"{_baseUrl}/get-by-warehouse-hotel-id/{warehouseHotelId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Fant ingen lagerenheter for dette lagerhotellet");
        }
        else if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var storageUnits = JsonSerializer.Deserialize<List<StorageUnit>>(content, options);
            return storageUnits;
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    public async Task<StorageUnit> GetStorageUnit(string id, string token)
    {
        string url = _baseUrl + $"/get-by-id/{id}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string deserializedResponseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserializedResponse = JsonSerializer.Deserialize<StorageUnit>(deserializedResponseString, options);
            return deserializedResponse;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Fant ingen lagerenhet med denne id'en");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    public async Task OccupyStorageUnit(string id, string userId)
    {
        string url = _baseUrl + $"/occupy";
        var request = new OccupyStorageUnitRequest(id, userId);
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Fant ingen lagerenhet med denne id'en");
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException("Lagerenheten er allerede opptatt");
            }
            else
            {
                throw new Exception("Noe gikk galt");
            }
        }
    }

    public async Task<List<StorageUnit>> GetAllStorageUnits()
    {
        string url = _baseUrl;
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string deserializedResponseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserializedResponse = JsonSerializer.Deserialize<List<StorageUnit>>(deserializedResponseString, options);
            return deserializedResponse;
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    public async Task DeleteStorageUnit(string id)
    {
        string url = _baseUrl + $"/{id}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await client.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Fant ingen lagerenhet med denne id'en");
            }
            else
            {
                throw new Exception("Noe gikk galt");
            }
        }
    }

    public async Task ModifyStorageUnit(StorageUnit updatedStorageUnit)
    {
        string url = _baseUrl + $"/modify";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        var request = new ModifyStorageUnitRequest(updatedStorageUnit);
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Fant ingen lagerenhet med denne id'en");
            }
            else
            {
                throw new Exception("Noe gikk galt");
            }
        }
    }
}
