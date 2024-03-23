using LagerhotellAPI.Models.CustomExceptionModels;

namespace Lagerhotell.Services;

public class StorageUnitService
{
    private readonly HttpClient client = new HttpClient();
    private readonly string _baseUrl = "https://localhost:7272/storage-units";

    /// <summary>
    /// Adds a storage unit to the database
    /// <para>The endpoint that is called can only be called by administrators</para>
    /// </summary>
    /// <param name="storageUnitRequest"></param>
    /// <returns>Storage unit Id</returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> AddStorageUnit(StorageUnit storageUnitRequest, string token)
    {
        string url = _baseUrl + "/add";
        CreateStorageUnitRequest request = new CreateStorageUnitRequest { StorageUnit = storageUnitRequest };
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string deserializedResponse = await response.Content.ReadAsStringAsync();
            string storageUnitId = JsonSerializer.Deserialize<CreateStorageUnitResponse>(deserializedResponse).StorageUnitId;
            return storageUnitId;
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
            var storageUnits = JsonSerializer.Deserialize<List<StorageUnit>>(content);
            return storageUnits;
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }
}
