namespace Lagerhotell.Services;

public class AssetService
{
    private readonly HttpClient client = new();
    private readonly string _baseUrl = "https://localhost:7272/assets";

    /// <summary>
    /// Gets all linked assets to a warehouse hotel
    /// </summary>
    /// <param name="warehouseHotelId"></param>
    /// <returns>List of assets converted to base64 strings</returns>
    public async Task<List<string>> GetWarehouseHotelImagecarouselImages(string warehouseHotelId)
    {
        string url = _baseUrl + $"/0/0/{warehouseHotelId}";
        HttpResponseMessage response = await client.GetAsync(url);
        string responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        List<ImageAsset> responseImageAssets = JsonSerializer.Deserialize<GetAllAssetsResponse>(responseString, options).Assets;
        List<string> imagesBase64 = new();
        foreach (ImageAsset imageAsset in responseImageAssets)
        {
            imagesBase64.Add(Convert.ToBase64String(imageAsset.ImageBytes));
        }
        return imagesBase64;
    }

    /// <summary>
    /// Gets all linked assets to a warehouse hotel
    /// </summary>
    /// <param name="warehouseHotelId"></param>
    /// <returns>A list of image assets</returns>
    public async Task<List<ImageAsset>> GetLinkedWarehouseHotelImages(string warehouseHotelId)
    {
        string url = _baseUrl + $"/0/0/{warehouseHotelId}";
        HttpResponseMessage response = await client.GetAsync(url);
        string responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        List<ImageAsset> responseImageAssets = JsonSerializer.Deserialize<GetAllAssetsResponse>(responseString, options).Assets;
        return responseImageAssets;
    }
}
