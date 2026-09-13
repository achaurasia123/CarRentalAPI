using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminDashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/dashboard/summary
    [HttpGet("summary")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> GetSummary()
    {
        var totalCars = await _db.MstCars.CountAsync();
        var availableCars = await _db.MstCars.CountAsync(c => c.IsAvailable);
        var totalDrivers = await _db.MstDrivers.CountAsync();
        var activeDrivers = await _db.MstDrivers.CountAsync(d => d.IsActive);

        var totalBookings = await _db.TrnBookings.CountAsync(b => b.Status != "Cancelled");

        var today = DateTime.UtcNow.Date;
        var todayBookings = await _db.TrnBookings.CountAsync(b =>
            b.Status != "Cancelled" && b.FromDate.Date <= today && b.ToDate.Date >= today);

        var totalRevenue = await _db.TrnBookings
            .Where(b => b.Status != "Cancelled")
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;

        return Ok(new AdminDashboardSummaryDto
        {
            TotalCars = totalCars,
            AvailableCars = availableCars,
            BookedCars = totalCars - availableCars,
            TotalDrivers = totalDrivers,
            ActiveDrivers = activeDrivers,
            TotalBookings = totalBookings,
            TodayBookings = todayBookings,
            TotalRevenue = totalRevenue
        });
    }

    // GET /api/admin/dashboard/car-status?date=2026-09-12
    // For a given date: how many cars total, how many have an active
    // (non-cancelled) booking that covers that date, and how many are free -
    // plus, per car, which booking (if any) is holding it that day.
    [HttpGet("car-status")]
    public async Task<ActionResult<CarStatusSummaryDto>> GetCarStatus([FromQuery] DateTime? date)
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;

        var activeBookings = await _db.TrnBookings
            .Include(b => b.User)
            .Where(b => b.Status != "Cancelled" && b.FromDate.Date <= targetDate && b.ToDate.Date >= targetDate)
            .ToListAsync();

        var bookingByCarId = activeBookings
            .GroupBy(b => b.CarId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.CreatedDate).First());

        var cars = await _db.MstCars
            .Include(c => c.Location)
            .Include(c => c.AssignedDriver)
            .OrderBy(c => c.CarId)
            .ToListAsync();

        var carStatuses = cars.Select(c =>
        {
            bookingByCarId.TryGetValue(c.CarId, out var booking);
            return new CarStatusForDateDto
            {
                CarId = c.CarId,
                CarName = c.CarName,
                Brand = c.Brand,
                Location = c.Location?.CityName ?? string.Empty,
                DriverName = c.AssignedDriver?.DriverName,
                IsBooked = booking is not null,
                BookingNo = booking?.BookingNo,
                CustomerName = booking?.User?.FullName
            };
        }).ToList();

        return Ok(new CarStatusSummaryDto
        {
            Date = targetDate,
            TotalCars = carStatuses.Count,
            BookedCars = carStatuses.Count(c => c.IsBooked),
            AvailableCars = carStatuses.Count(c => !c.IsBooked),
            Cars = carStatuses
        });
    }
}
