namespace CarRentalAPI.Dtos.Admin;

// Top-of-dashboard summary tiles.
public class AdminDashboardSummaryDto
{
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int BookedCars { get; set; }
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}

// One row of the "car status for a chosen date" table.
public class CarStatusForDateDto
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public bool IsBooked { get; set; }
    public string? BookingNo { get; set; }
    public string? CustomerName { get; set; }
}

// The full response for GET /api/admin/dashboard/car-status?date=...
public class CarStatusSummaryDto
{
    public DateTime Date { get; set; }
    public int TotalCars { get; set; }
    public int BookedCars { get; set; }
    public int AvailableCars { get; set; }
    public List<CarStatusForDateDto> Cars { get; set; } = new();
}
