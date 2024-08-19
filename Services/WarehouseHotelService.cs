using Microsoft.AspNetCore.Components.Forms;

namespace Lagerhotell.Services;

public class WarehouseHotelService
{
    private readonly string _baseUrl = "https://localhost:7272/warehouse-hotels";
    private readonly HttpClient client = new();
    private FileHandler _fileHandler { get; set; } = new();
    private SessionService _sessionService { get; set; }
    public WarehouseHotelService(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Legger til et nytt lagerhotell i databasen
    /// </summary>
    /// <param name="warehouseHotel"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> AddWarehouseHotel(WarehouseHotel warehouseHotel, List<IBrowserFile> images, string token)
    {
        // The list of images should be validated somewhere
        List<ImageAsset> newImages = new();
        try
        {
            foreach (var image in images)
            {
                ImageAsset imageAsset = new()
                {
                    ImageBytes = await _fileHandler.ConvertToByteArray(image),
                    Name = image.Name,
                };
                newImages.Add(imageAsset);
            }
        }
        catch (IOException e)
        {
            throw new IOException("Error converting file to byte array, file is likely too large", e);
        }
        var request = new AddWarehouseHotelRequest(warehouseHotel, newImages);
        string url = _baseUrl + "/add";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PostAsync(url, content);
        string deserializedResponse;
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
        if (response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();
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
    public async Task<string> ChangeWarehouseHotel(string oldWarehouseHotelName, WarehouseHotel newWarehouseHotel, List<IBrowserFile> images, string token)
    {
        // The list of images should be validated somewhere
        List<ImageAsset> newImages = new();
        foreach (var image in images)
        {
            ImageAsset imageAsset = new()
            {
                ImageBytes = await _fileHandler.ConvertToByteArray(image),
                Name = image.Name,
            };
            newImages.Add(imageAsset);
        }
        var request = new ModifyWarehouseHotelRequest(newWarehouseHotel, oldWarehouseHotelName, newImages);
        string url = _baseUrl + "/modify";
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new(jsonData, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.PutAsync(url, content);
        string deserializedResponse;
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

    public async Task<WarehouseHotel> GetWarehouseHotel(string id)
    {
        string url = _baseUrl + $"/{id}";
        HttpResponseMessage responseMessage = await client.GetAsync(url);
        if (responseMessage.IsSuccessStatusCode)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            GetWarehouseHotelByIdResponse warehouseHotel = JsonSerializer.Deserialize<GetWarehouseHotelByIdResponse>(responseContent, options);
            return warehouseHotel.WarehouseHotel;
        }
        else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }

    public async Task<string> GetWarehouseHotelName(string id)
    {
        string url = _baseUrl + $"/{id}";
        HttpResponseMessage responseMessage = await client.GetAsync(url);
        if (responseMessage.IsSuccessStatusCode)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            GetWarehouseHotelByIdResponse warehouseHotel = JsonSerializer.Deserialize<GetWarehouseHotelByIdResponse>(responseContent, options);
            return warehouseHotel.WarehouseHotel.Name;
        }
        else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Lagerhotellet eksisterer ikke");
        }
        else
        {
            throw new Exception("Noe gikk galt");
        }
    }
}