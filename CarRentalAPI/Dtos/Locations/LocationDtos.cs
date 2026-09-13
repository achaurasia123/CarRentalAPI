namespace CarRentalAPI.Dtos.Locations;

public class LocationDto
{
    public int LocationId { get; set; }
    public string CityName { get; set; } = string.Empty;
}

public class DistancePriceDto
{
    public int FromLocationId { get; set; }
    public string FromLocationName { get; set; } = string.Empty;
    public int ToLocationId { get; set; }
    public string ToLocationName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal EstimatedAmount { get; set; }
}
