using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using CarRentalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Full driver CRUD for the admin "Manage Drivers" screens.
[ApiController]
[Route("api/admin/drivers")]
[Authorize(Roles = "Admin")]
public class AdminDriversController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminDriversController(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<MstDriver> DriversWithCars() => _db.MstDrivers.Include(d => d.Cars);

    // GET /api/admin/drivers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminDriverDto>>> GetDrivers()
    {
        var drivers = await DriversWithCars().OrderBy(d => d.DriverId).ToListAsync();
        return Ok(drivers.Select(ToDto));
    }

    // GET /api/admin/drivers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminDriverDto>> GetDriver(int id)
    {
        var driver = await DriversWithCars().FirstOrDefaultAsync(d => d.DriverId == id);
        if (driver is null) return NotFound();

        return Ok(ToDto(driver));
    }

    // POST /api/admin/drivers
    [HttpPost]
    public async Task<ActionResult<AdminDriverDto>> CreateDriver([FromBody] SaveDriverDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var driver = new MstDriver
        {
            DriverName = request.DriverName,
            PhoneNumber = request.PhoneNumber,
            LicenseNumber = request.LicenseNumber,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        _db.MstDrivers.Add(driver);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDriver), new { id = driver.DriverId }, new AdminDriverDto
        {
            DriverId = driver.DriverId,
            DriverName = driver.DriverName,
            PhoneNumber = driver.PhoneNumber,
            LicenseNumber = driver.LicenseNumber,
            IsActive = driver.IsActive,
            AssignedCarsCount = 0,
            AssignedCarNames = string.Empty
        });
    }

    // PUT /api/admin/drivers/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminDriverDto>> UpdateDriver(int id, [FromBody] SaveDriverDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var driver = await _db.MstDrivers.FindAsync(id);
        if (driver is null) return NotFound();

        driver.DriverName = request.DriverName;
        driver.PhoneNumber = request.PhoneNumber;
        driver.LicenseNumber = request.LicenseNumber;
        driver.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        var saved = await DriversWithCars().FirstAsync(d => d.DriverId == id);
        return Ok(ToDto(saved));
    }

    // DELETE /api/admin/drivers/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        var driver = await _db.MstDrivers.FindAsync(id);
        if (driver is null) return NotFound();

        var assignedCar = await _db.MstCars.Where(c => c.AssignedDriverId == id).Select(c => c.CarName).FirstOrDefaultAsync();
        if (assignedCar is not null)
        {
            return Conflict(new
            {
                message = $"{driver.DriverName} is currently assigned to {assignedCar}. Unassign them from the car first."
            });
        }

        _db.MstDrivers.Remove(driver);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static AdminDriverDto ToDto(MstDriver d) => new()
    {
        DriverId = d.DriverId,
        DriverName = d.DriverName,
        PhoneNumber = d.PhoneNumber,
        LicenseNumber = d.LicenseNumber,
        IsActive = d.IsActive,
        AssignedCarsCount = d.Cars.Count,
        AssignedCarNames = string.Join(", ", d.Cars.Select(c => c.CarName))
    };
}
