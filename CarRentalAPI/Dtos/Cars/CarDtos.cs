namespace CarRentalAPI.Dtos.Cars;

public class CarDto
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string CarType { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int Seats { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class CarFilterDto
{
    public string? Search { get; set; }
    public string? CarType { get; set; }
}

// Returned by GET /api/cars/{id}/detail - everything the "Book Now" detail
// page shows before the customer picks a from/to location: full car info,
// rating, and the driver that's already fixed/assigned to this car.
public class CarDetailDto : CarDto
{
    public string DriverName { get; set; } = string.Empty;
}
