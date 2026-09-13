using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using CarRentalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Cities (MstLocation) + the distance matrix between them (MstLocationDistance)
// that FarePricing / BookingsController / LocationsController all rely on for
// the location-based "Book Now" fare calculation.
[ApiController]
[Route("api/admin/locations")]
[Authorize(Roles = "Admin")]
public class AdminLocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminLocationsController(AppDbContext db)
    {
        _db = db;
    }

    // ---------- Cities ----------

    // GET /api/admin/locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationAdminDto>>> GetLocations()
    {
        var locations = await _db.MstLocations
            .OrderBy(l => l.CityName)
            .Select(l => new LocationAdminDto
            {
                LocationId = l.LocationId,
                CityName = l.CityName,
                CarsCount = l.Cars.Count
            })
            .ToListAsync();

        return Ok(locations);
    }

    // POST /api/admin/locations
    [HttpPost]
    public async Task<ActionResult<LocationAdminDto>> CreateLocation([FromBody] SaveLocationDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cityName = request.CityName.Trim();
        var exists = await _db.MstLocations.AnyAsync(l => l.CityName.ToLower() == cityName.ToLower());
        if (exists)
        {
            return Conflict(new { message = $"{cityName} already exists." });
        }

        var location = new MstLocation { CityName = cityName };
        _db.MstLocations.Add(location);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocations), new LocationAdminDto
        {
            LocationId = location.LocationId,
            CityName = location.CityName,
            CarsCount = 0
        });
    }

    // PUT /api/admin/locations/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LocationAdminDto>> UpdateLocation(int id, [FromBody] SaveLocationDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var location = await _db.MstLocations.FindAsync(id);
        if (location is null) return NotFound();

        var cityName = request.CityName.Trim();
        var duplicate = await _db.MstLocations
            .AnyAsync(l => l.LocationId != id && l.CityName.ToLower() == cityName.ToLower());
        if (duplicate)
        {
            return Conflict(new { message = $"{cityName} already exists." });
        }

        location.CityName = cityName;
        await _db.SaveChangesAsync();

        var carsCount = await _db.MstCars.CountAsync(c => c.LocationId == id);
        return Ok(new LocationAdminDto { LocationId = location.LocationId, CityName = location.CityName, CarsCount = carsCount });
    }

    // DELETE /api/admin/locations/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _db.MstLocations.FindAsync(id);
        if (location is null) return NotFound();

        var carsUsingIt = await _db.MstCars.CountAsync(c => c.LocationId == id);
        if (carsUsingIt > 0)
        {
            return Conflict(new
            {
                message = $"{location.CityName} has {carsUsingIt} car(s) based there. Reassign them to another location first."
            });
        }

        var distancesUsingIt = await _db.MstLocationDistances
            .AnyAsync(d => d.FromLocationId == id || d.ToLocationId == id);
        if (distancesUsingIt)
        {
            return Conflict(new { message = $"{location.CityName} still has distance rows configured. Remove those first." });
        }

        var bookingDetailsUsingIt = await _db.TrnBookingDetails
            .AnyAsync(d => d.FromLocationId == id || d.ToLocationId == id);
        if (bookingDetailsUsingIt)
        {
            return Conflict(new { message = $"{location.CityName} appears in past booking history and can't be deleted." });
        }

        _db.MstLocations.Remove(location);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ---------- Distances ----------

    // GET /api/admin/locations/distances
    // Each unordered city pair is shown once (canonical direction = the row
    // where FromLocationId < ToLocationId); both directions still exist in
    // the database underneath and stay in sync automatically.
    [HttpGet("distances")]
    public async Task<ActionResult<IEnumerable<LocationDistanceDto>>> GetDistances()
    {
        var distances = await _db.MstLocationDistances
            .Include(d => d.FromLocation)
            .Include(d => d.ToLocation)
            .Where(d => d.FromLocationId < d.ToLocationId)
            .OrderBy(d => d.FromLocation!.CityName)
            .ThenBy(d => d.ToLocation!.CityName)
            .ToListAsync();

        return Ok(distances.Select(ToDistanceDto));
    }

    // POST /api/admin/locations/distances
    [HttpPost("distances")]
    public async Task<ActionResult<LocationDistanceDto>> CreateDistance([FromBody] SaveLocationDistanceDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.FromLocationId == request.ToLocationId)
        {
            return BadRequest(new { message = "From and To locations must be different." });
        }

        var from = await _db.MstLocations.FindAsync(request.FromLocationId);
        var to = await _db.MstLocations.FindAsync(request.ToLocationId);
        if (from is null || to is null)
        {
            return BadRequest(new { message = "Please choose valid from/to locations." });
        }

        var alreadyExists = await _db.MstLocationDistances.AnyAsync(d =>
            (d.FromLocationId == request.FromLocationId && d.ToLocationId == request.ToLocationId) ||
            (d.FromLocationId == request.ToLocationId && d.ToLocationId == request.FromLocationId));

        if (alreadyExists)
        {
            return Conflict(new { message = $"A distance between {from.CityName} and {to.CityName} is already configured." });
        }

        // Both directions are stored explicitly (matches the seed data
        // pattern) so exact-match lookups elsewhere in the app keep working.
        var forward = new MstLocationDistance
        {
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            DistanceKm = request.DistanceKm
        };
        var backward = new MstLocationDistance
        {
            FromLocationId = request.ToLocationId,
            ToLocationId = request.FromLocationId,
            DistanceKm = request.DistanceKm
        };

        _db.MstLocationDistances.AddRange(forward, backward);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDistances), new LocationDistanceDto
        {
            DistanceId = forward.DistanceId,
            FromLocationId = forward.FromLocationId,
            FromCityName = from.CityName,
            ToLocationId = forward.ToLocationId,
            ToCityName = to.CityName,
            DistanceKm = forward.DistanceKm
        });
    }

    // PUT /api/admin/locations/distances/5 - updates the km value for a pair
    // and keeps the mirrored reverse row in sync so both directions always agree.
    [HttpPut("distances/{id:int}")]
    public async Task<ActionResult<LocationDistanceDto>> UpdateDistance(int id, [FromBody] UpdateLocationDistanceDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var distance = await _db.MstLocationDistances
            .Include(d => d.FromLocation)
            .Include(d => d.ToLocation)
            .FirstOrDefaultAsync(d => d.DistanceId == id);
        if (distance is null) return NotFound();

        var mirror = await _db.MstLocationDistances.FirstOrDefaultAsync(d =>
            d.FromLocationId == distance.ToLocationId && d.ToLocationId == distance.FromLocationId);

        distance.DistanceKm = request.DistanceKm;
        if (mirror is not null) mirror.DistanceKm = request.DistanceKm;

        await _db.SaveChangesAsync();

        return Ok(ToDistanceDto(distance));
    }

    // DELETE /api/admin/locations/distances/5 - removes both directions.
    [HttpDelete("distances/{id:int}")]
    public async Task<IActionResult> DeleteDistance(int id)
    {
        var distance = await _db.MstLocationDistances.FindAsync(id);
        if (distance is null) return NotFound();

        var mirror = await _db.MstLocationDistances.FirstOrDefaultAsync(d =>
            d.FromLocationId == distance.ToLocationId && d.ToLocationId == distance.FromLocationId);

        _db.MstLocationDistances.Remove(distance);
        if (mirror is not null) _db.MstLocationDistances.Remove(mirror);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static LocationDistanceDto ToDistanceDto(MstLocationDistance d) => new()
    {
        DistanceId = d.DistanceId,
        FromLocationId = d.FromLocationId,
        FromCityName = d.FromLocation?.CityName ?? string.Empty,
        ToLocationId = d.ToLocationId,
        ToCityName = d.ToLocation?.CityName ?? string.Empty,
        DistanceKm = d.DistanceKm
    };
}
