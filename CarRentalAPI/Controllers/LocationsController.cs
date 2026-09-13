using CarRentalAPI.Common;
using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LocationsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/locations -> populates the from/to dropdowns on the Book Now page
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
    {
        var locations = await _db.MstLocations
            .OrderBy(l => l.CityName)
            .Select(l => new LocationDto { LocationId = l.LocationId, CityName = l.CityName })
            .ToListAsync();

        return Ok(locations);
    }

    // GET /api/locations/price?fromLocationId=1&toLocationId=4
    // Live distance + price preview shown on the Book Now page as soon as both
    // locations are picked - same formula CreateDetailedBooking() uses, so the
    // preview never disagrees with what actually gets charged.
    [HttpGet("price")]
    public async Task<ActionResult<DistancePriceDto>> GetPrice([FromQuery] int fromLocationId, [FromQuery] int toLocationId)
    {
        var from = await _db.MstLocations.FindAsync(fromLocationId);
        var to = await _db.MstLocations.FindAsync(toLocationId);

        if (from is null || to is null)
        {
            return NotFound(new { message = "Unknown from/to location." });
        }

        decimal distanceKm;
        if (fromLocationId == toLocationId)
        {
            distanceKm = 0m;
        }
        else
        {
            var distanceRow = await _db.MstLocationDistances
                .FirstOrDefaultAsync(d => d.FromLocationId == fromLocationId && d.ToLocationId == toLocationId);

            if (distanceRow is null)
            {
                return NotFound(new { message = $"No distance is configured between {from.CityName} and {to.CityName} yet." });
            }

            distanceKm = distanceRow.DistanceKm;
        }

        return Ok(new DistancePriceDto
        {
            FromLocationId = from.LocationId,
            FromLocationName = from.CityName,
            ToLocationId = to.LocationId,
            ToLocationName = to.CityName,
            DistanceKm = distanceKm,
            EstimatedAmount = FarePricing.CalculateAmount(distanceKm)
        });
    }
}
