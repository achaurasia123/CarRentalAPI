using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/dashboard/stats
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var stats = new DashboardStatsDto
        {
            TotalCars = await _db.MstCars.CountAsync(),
            AvailableCars = await _db.MstCars.CountAsync(c => c.IsAvailable),
            ActiveBookings = await _db.TrnBookings.CountAsync(b => b.Status == "Confirmed"),
            TotalRevenue = await _db.TrnBookings
                .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m
        };

        return Ok(stats);
    }
}
