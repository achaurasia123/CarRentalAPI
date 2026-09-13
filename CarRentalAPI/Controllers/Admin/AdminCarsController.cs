using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using CarRentalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Full car CRUD for the admin "Manage Cars" screens. Everything here is
// restricted to the Admin role - the JWT already carries a Role claim
// (see TokenService), so [Authorize(Roles = "Admin")] is all that's needed.
//
// Note: entities are always loaded first (with Include) and THEN mapped to
// AdminCarDto in memory via ToDto(). EF Core cannot translate an arbitrary
// C# method call into SQL inside .Select(), so the mapping step is kept out
// of the database query on purpose.
[ApiController]
[Route("api/admin/cars")]
[Authorize(Roles = "Admin")]
public class AdminCarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCarsController(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<MstCar> CarsWithDetails() =>
        _db.MstCars
            .Include(c => c.CarType)
            .Include(c => c.Location)
            .Include(c => c.AssignedDriver);

    // GET /api/admin/cars
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminCarDto>>> GetCars()
    {
        var cars = await CarsWithDetails().OrderBy(c => c.CarId).ToListAsync();
        return Ok(cars.Select(ToDto));
    }

    // GET /api/admin/cars/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminCarDto>> GetCar(int id)
    {
        var car = await CarsWithDetails().FirstOrDefaultAsync(c => c.CarId == id);
        if (car is null) return NotFound();

        return Ok(ToDto(car));
    }

    // POST /api/admin/cars
    [HttpPost]
    public async Task<ActionResult<AdminCarDto>> CreateCar([FromBody] SaveCarDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var validation = await ValidateReferencesAsync(request);
        if (validation is not null) return validation;

        var car = new MstCar
        {
            CarName = request.CarName,
            Brand = request.Brand,
            CarTypeId = request.CarTypeId,
            PricePerDay = request.PricePerDay,
            Seats = request.Seats,
            Transmission = request.Transmission,
            FuelType = request.FuelType,
            Rating = request.Rating,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable,
            LocationId = request.LocationId,
            AssignedDriverId = request.AssignedDriverId,
            CreatedDate = DateTime.UtcNow
        };

        _db.MstCars.Add(car);
        await _db.SaveChangesAsync();

        var saved = await CarsWithDetails().FirstAsync(c => c.CarId == car.CarId);
        return CreatedAtAction(nameof(GetCar), new { id = car.CarId }, ToDto(saved));
    }

    // PUT /api/admin/cars/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminCarDto>> UpdateCar(int id, [FromBody] SaveCarDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var car = await _db.MstCars.FindAsync(id);
        if (car is null) return NotFound();

        var validation = await ValidateReferencesAsync(request);
        if (validation is not null) return validation;

        car.CarName = request.CarName;
        car.Brand = request.Brand;
        car.CarTypeId = request.CarTypeId;
        car.PricePerDay = request.PricePerDay;
        car.Seats = request.Seats;
        car.Transmission = request.Transmission;
        car.FuelType = request.FuelType;
        car.Rating = request.Rating;
        car.ImageUrl = request.ImageUrl;
        car.IsAvailable = request.IsAvailable;
        car.LocationId = request.LocationId;
        car.AssignedDriverId = request.AssignedDriverId;

        await _db.SaveChangesAsync();

        var saved = await CarsWithDetails().FirstAsync(c => c.CarId == id);
        return Ok(ToDto(saved));
    }

    // DELETE /api/admin/cars/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var car = await _db.MstCars.FindAsync(id);
        if (car is null) return NotFound();

        var hasBookings = await _db.TrnBookings.AnyAsync(b => b.CarId == id);
        if (hasBookings)
        {
            return Conflict(new
            {
                message = $"{car.CarName} has booking history and can't be deleted. Mark it unavailable instead."
            });
        }

        _db.MstCars.Remove(car);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidateReferencesAsync(SaveCarDto request)
    {
        if (!await _db.MstCarTypes.AnyAsync(t => t.CarTypeId == request.CarTypeId))
        {
            return BadRequest(new { message = "Please choose a valid car type." });
        }

        if (!await _db.MstLocations.AnyAsync(l => l.LocationId == request.LocationId))
        {
            return BadRequest(new { message = "Please choose a valid location." });
        }

        if (request.AssignedDriverId.HasValue &&
            !await _db.MstDrivers.AnyAsync(d => d.DriverId == request.AssignedDriverId.Value))
        {
            return BadRequest(new { message = "Please choose a valid driver." });
        }

        return null;
    }

    private static AdminCarDto ToDto(MstCar c) => new()
    {
        CarId = c.CarId,
        CarName = c.CarName,
        Brand = c.Brand,
        CarTypeId = c.CarTypeId,
        CarType = c.CarType?.TypeName ?? string.Empty,
        PricePerDay = c.PricePerDay,
        Seats = c.Seats,
        Transmission = c.Transmission,
        FuelType = c.FuelType,
        Rating = c.Rating,
        ImageUrl = c.ImageUrl,
        IsAvailable = c.IsAvailable,
        LocationId = c.LocationId,
        Location = c.Location?.CityName ?? string.Empty,
        AssignedDriverId = c.AssignedDriverId,
        DriverName = c.AssignedDriver?.DriverName
    };
}
