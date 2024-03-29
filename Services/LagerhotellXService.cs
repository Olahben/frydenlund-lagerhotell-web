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
        if (newOpeningTime.Length == 4 && relevantWarehouseHotel.OpeningHours.Opens % 1 != 0)
        {
            newOpeningTime = newOpeningTime.Insert(2, ":");
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
        if (newClosingTime.Length == 4 && relevantWarehouseHotel.OpeningHours.Closes % 1 != 0)
        {
            newClosingTime = newClosingTime.Insert(2, ":");
            newClosingTime = newClosingTime.Remove(newClosingTime.Length - 2);
            newClosingTime += "30";
        }

        return (newOpeningTime, newClosingTime);
    }


    /// <summary>
    /// Returns a list of the unique area sizes of the provided list of storage units
    /// </summary>
    /// <param name="storageUnits"></param>
    /// <returns></returns>
    public async Task<List<double>> GetUniqueStorageUnitAreas(List<StorageUnit> storageUnits)
    {
        List<double> uniqueAreas = new();
        foreach (var storageUnit in storageUnits)
        {
            if (!uniqueAreas.Contains(storageUnit.Dimensions.Area))
            {
                uniqueAreas.Add(storageUnit.Dimensions.Area);
            }
        }
        return uniqueAreas;
    }
}
