using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Admin;

public class LocationAdminDto
{
    public int LocationId { get; set; }
    public string CityName { get; set; } = string.Empty;

    // How many cars are currently based at this location - shown so the
    // admin understands why a location can't be deleted.
    public int CarsCount { get; set; }
}

public class SaveLocationDto
{
    [Required, MaxLength(100)]
    public string CityName { get; set; } = string.Empty;
}

// One row of the admin "Distances" table - a single (unordered) city pair.
// Internally two MstLocationDistance rows exist (A->B and B->A) so the
// existing booking/price-lookup code - which always matches an exact
// from/to direction - keeps working unchanged; DistanceId here is the
// "canonical" A->B row's id, and AdminLocationsController keeps the
// mirrored B->A row in sync automatically.
public class LocationDistanceDto
{
    public int DistanceId { get; set; }
    public int FromLocationId { get; set; }
    public string FromCityName { get; set; } = string.Empty;
    public int ToLocationId { get; set; }
    public string ToCityName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
}

public class SaveLocationDistanceDto
{
    [Required]
    public int FromLocationId { get; set; }

    [Required]
    public int ToLocationId { get; set; }

    [Range(0.1, 100000)]
    public decimal DistanceKm { get; set; }
}

public class UpdateLocationDistanceDto
{
    [Range(0.1, 100000)]
    public decimal DistanceKm { get; set; }
}
