namespace Lagerhotell.Services;

public class AssetService
{
    private readonly HttpClient client = new();
    private readonly string _baseUrl = "https://localhost:7272/assets";
}
