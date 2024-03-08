namespace Lagerhotell.Services;

public class WarehouseHotelService
{
    private readonly string _baseUrl = "https://localhost:7272/warehouse-hotels";
    private readonly HttpClient client = new HttpClient();
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

    public async Task<(string, string)> DeleteWarehouseHotel(string warehouseHotelName, string token)
    {
        string getUrl = _baseUrl + $"/get-by-name/{warehouseHotelName}";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var warehouseHotelResponse = await client.GetAsync(getUrl);
        if (warehouseHotelResponse.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else if (warehouseHotelResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Noe gikk galt");
        }
        else if (warehouseHotelResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception("Noe gikk galt");
        }
        else if (warehouseHotelResponse.StatusCode == HttpStatusCode.OK)
        {
            string warehouseHotelResponseString = await warehouseHotelResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            GetWarehouseHotelByIdResponse warehouseHotel = JsonSerializer.Deserialize<GetWarehouseHotelByIdResponse>(warehouseHotelResponseString, options);

            string url = _baseUrl + $"/{warehouseHotel.WarehouseHotel.WarehouseHotelId}";
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
            return (warehouseHotel.WarehouseHotel.WarehouseHotelId, warehouseHotel.WarehouseHotel.Name);
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }

    }

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
}