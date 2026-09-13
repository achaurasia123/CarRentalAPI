namespace CarRentalAPI.Dtos.Dashboard;

public class DashboardStatsDto
{
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int ActiveBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}
