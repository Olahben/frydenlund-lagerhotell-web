using LagerhotellAPI.Models.FrontendModels.Custom;

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
    /// Returns a list of "StorageUnitSize" objects, which contains the unique storage unit sizes, separated by size and if they are temperated or not
    /// if there is no available storage uniits for the size, that will be displayed on the frontend
    /// </summary>
    /// <param name="storageUnits"></param>
    /// <returns></returns>
    public async Task<List<StorageUnitSize>> GetUniqueStorageUnitAreas(List<StorageUnit> storageUnits)
    {
        List<StorageUnitSize> storageUnitSizes = new();

        foreach (var storageUnit in storageUnits)
        {
            decimal area = (decimal)storageUnit.Dimensions.Area;
            area = Math.Round(area * 2, MidpointRounding.AwayFromZero) / 2;
            decimal volume = (decimal)storageUnit.Dimensions.Volume;
            volume = Math.Round(volume * 2, MidpointRounding.AwayFromZero) / 2;
            StorageUnitSize newSize = new();
            if (!storageUnit.Occupied)
            {
                newSize = new StorageUnitSize
                {
                    Area = storageUnit.Dimensions.Area,
                    Volume = storageUnit.Dimensions.Volume,
                    RoundedArea = area,
                    RoundedVolume = volume,
                    Price = storageUnit.PricePerMonth,
                    Temperated = storageUnit.Temperated,
                    storageUnitIds = new List<string> { storageUnit.StorageUnitId }
                };
            }
            else
            {
                newSize = new StorageUnitSize
                {
                    RoundedArea = area,
                    RoundedVolume = volume,
                    Volume = storageUnit.Dimensions.Volume,
                    Price = storageUnit.PricePerMonth,
                    Temperated = storageUnit.Temperated,
                    storageUnitIds = new List<string> { }
                };
                storageUnit.StorageUnitId = null;
            }

            if (!storageUnitSizes.Any(storageUnitSizes => storageUnitSizes.Area == newSize.Area))
            {
                storageUnitSizes.Add(newSize);
            }
            else if (storageUnitSizes.Any(storageUnitSizes => storageUnitSizes.Area == newSize.Area && storageUnitSizes.Temperated != newSize.Temperated))
            {
                storageUnitSizes.Add(newSize);
            }
            else if (storageUnitSizes.Any(storageUnitSizes => storageUnitSizes.Area == newSize.Area))
            {
                if (storageUnit.StorageUnitId != null)
                {
                    storageUnitSizes.First(storageUnitSize => storageUnitSize.Area == newSize.Area).storageUnitIds.Add(storageUnit.StorageUnitId);
                }
            }

        }
        storageUnitSizes = storageUnitSizes.OrderBy(storageUnitSizes => storageUnitSizes.Price.Amount).ToList();
        return storageUnitSizes;
    }
}
