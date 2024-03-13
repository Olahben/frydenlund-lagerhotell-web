namespace Lagerhotell.Services;

public class WarehouseHotelService
{
    private readonly string _baseUrl = "https://localhost:7272/warehouse-hotels";
    private readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Legger til et nytt lagerhotell i databasen
    /// </summary>
    /// <param name="warehouseHotel"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> AddWarehouseHotel(WarehouseHotel warehouseHotel, string token)
    {
        var request = new AddWarehouseHotelRequest(warehouseHotel);
        string url = _baseUrl + "/add";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PostAsync(url, content);
        string deserializedResponse = string.Empty;
        if (response.IsSuccessStatusCode)
        {
            deserializedResponse = await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
        return deserializedResponse;
    }

    /// <summary>
    /// Sletter lagerhotellet med det gitte navnet
    /// <para>Endepunktet som blir kallet kan bare bli kallet på av administratorere</para>
    /// </summary>
    /// <param name="warehouseHotelName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<(string, string)> DeleteWarehouseHotel(string warehouseHotelName, string token)
    {
        WarehouseHotel warehouseHotel = await GetWarehouseHotelByName(warehouseHotelName, token);

        string url = _baseUrl + $"/{warehouseHotel.WarehouseHotelId}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.DeleteAsync(url);
        string deserializedResponse = string.Empty;
        if (response.IsSuccessStatusCode)
        {
            deserializedResponse = await response.Content.ReadAsStringAsync();
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Noe er galt med JWT");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
        return (warehouseHotel.WarehouseHotelId, warehouseHotel.Name);
    }


    /// <summary>
    /// <para>
    /// Endrer lagerhotellet med det gitte navnet til det nye lagerhotellet som er gitt.
    /// </para>
    /// ID-en til det gamle lagerhotellet beholdes
    /// <para>Endepunktet som blir kallet kan bare bli kallet på av administratorerere</para>
    /// </summary>
    /// <param name="oldWarehouseHotelName"></param>
    /// <param name="newWarehouseHotel"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> ChangeWarehouseHotel(string oldWarehouseHotelName, WarehouseHotel newWarehouseHotel, string token)
    {
        var request = new ModifyWarehouseHotelRequest(newWarehouseHotel, oldWarehouseHotelName);
        string url = _baseUrl + "/modify";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PutAsync(url, content);
        string deserializedResponse = string.Empty;
        if (response.IsSuccessStatusCode)
        {
            deserializedResponse = await response.Content.ReadAsStringAsync();
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
        return deserializedResponse;
    }

    /// <summary>
    /// Henter lagerhotellet med det gitte navnet og returnerer det som domeneobjekt
    /// </summary>
    /// <param name="warehouseHotelName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="Exception"></exception>

    public async Task<WarehouseHotel> GetWarehouseHotelByName(string warehouseHotelName, string token)
    {
        string url = _baseUrl + $"/get-by-name/{warehouseHotelName}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            GetWarehouseHotelByIdResponse warehouseHotel = JsonSerializer.Deserialize<GetWarehouseHotelByIdResponse>(responseContent, options);
            return warehouseHotel.WarehouseHotel;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    /// <summary>
    /// Henter alle lagerhotellene i databasen og returnerer dem som en liste av domeneobjekter
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="Exception"></exception>

    public async Task<List<WarehouseHotel>> GetAllWarehouseHotels()
    {
        string url = _baseUrl;

        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            GetAllWarehouseHotelsResponse warehouseHotels = JsonSerializer.Deserialize<GetAllWarehouseHotelsResponse>(responseContent, options);
            return warehouseHotels.WarehouseHotels;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Ingen lagerhotell ble funnet");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }
}