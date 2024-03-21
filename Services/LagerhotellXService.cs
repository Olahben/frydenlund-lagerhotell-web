namespace Lagerhotell.Services;

public class LagerhotellXService
{
    private readonly HttpClient client = new();
    private readonly string _baseUrl = "https://localhost:7272/locations";

    public (string, string) CalculateTime(string openingTime, string closingTime, WarehouseHotel relevantWarehouseHotel)
    {
        string newOpeningTime = openingTime;
        string newClosingTime = closingTime;

        if (newOpeningTime.Length == 1)
        {
            newOpeningTime = "0" + newOpeningTime;
        }
        if (newClosingTime.Length == 1)
        {
            newClosingTime = newClosingTime.Insert(0, "0");
            newClosingTime = "0" + newClosingTime;
        }
        if (newOpeningTime.Length == 2 && !(relevantWarehouseHotel.OpeningHours.Opens % 1 != 0))
        {
            newOpeningTime += ":00";
        }
        if (newOpeningTime.Length == 2 && relevantWarehouseHotel.OpeningHours.Opens % 1 != 0)
        {
            newOpeningTime += ":30";
        }
        if (newOpeningTime.Length == 3 && relevantWarehouseHotel.OpeningHours.Opens % 1 != 0)
        {
            newOpeningTime = newOpeningTime.Insert(0, "0");
            newOpeningTime = newOpeningTime.Insert(1, ":");
            newOpeningTime = newOpeningTime.Remove(newOpeningTime.Length - 2);
            newOpeningTime += "30";
        }
        if (newClosingTime.Length == 2 && !(relevantWarehouseHotel.OpeningHours.Closes % 1 != 0))
        {
            newClosingTime += ":00";
        }
        if (newClosingTime.Length == 2 && relevantWarehouseHotel.OpeningHours.Closes % 1 != 0)
        {
            newClosingTime += ":30";
        }
        if (newClosingTime.Length == 3 && relevantWarehouseHotel.OpeningHours.Closes % 1 != 0)
        {
            newClosingTime = newClosingTime.Insert(0, "0");
            newClosingTime = newClosingTime.Insert(1, ":");
            newClosingTime = newClosingTime.Remove(newClosingTime.Length - 2);
            newClosingTime += "30";
        }

        return (newOpeningTime, newClosingTime);
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
