using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Cars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CarsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/cars?search=swift&carType=Sedan
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetCars([FromQuery] string? search, [FromQuery] string? carType)
    {
        var query = _db.MstCars
            .Include(c => c.CarType)
            .Include(c => c.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.CarName.Contains(search) || c.Brand.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(carType) && carType != "All")
        {
            query = query.Where(c => c.CarType != null && c.CarType.TypeName == carType);
        }

        var cars = await query
            .OrderByDescending(c => c.Rating)
            .Select(c => new CarDto
            {
                CarId = c.CarId,
                CarName = c.CarName,
                Brand = c.Brand,
                CarType = c.CarType!.TypeName,
                PricePerDay = c.PricePerDay,
                Seats = c.Seats,
                Transmission = c.Transmission,
                FuelType = c.FuelType,
                Rating = c.Rating,
                ImageUrl = c.ImageUrl,
                IsAvailable = c.IsAvailable,
                Location = c.Location!.CityName
            })
            .ToListAsync();

        return Ok(cars);
    }

    // GET /api/cars/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CarDto>> GetCar(int id)
    {
        var car = await _db.MstCars
            .Include(c => c.CarType)
            .Include(c => c.Location)
            .Where(c => c.CarId == id)
            .Select(c => new CarDto
            {
                CarId = c.CarId,
                CarName = c.CarName,
                Brand = c.Brand,
                CarType = c.CarType!.TypeName,
                PricePerDay = c.PricePerDay,
                Seats = c.Seats,
                Transmission = c.Transmission,
                FuelType = c.FuelType,
                Rating = c.Rating,
                ImageUrl = c.ImageUrl,
                IsAvailable = c.IsAvailable,
                Location = c.Location!.CityName
            })
            .FirstOrDefaultAsync();

        if (car is null) return NotFound();

        return Ok(car);
    }

    // GET /api/cars/types
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<string>>> GetCarTypes()
    {
        var types = await _db.MstCarTypes.Select(t => t.TypeName).ToListAsync();
        return Ok(types);
    }

    // GET /api/cars/5/detail -> full car info + rating + the driver already
    // fixed/assigned to this car, shown on the "Book Now" detail page.
    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<CarDetailDto>> GetCarDetail(int id)
    {
        var car = await _db.MstCars
            .Include(c => c.CarType)
            .Include(c => c.Location)
            .Include(c => c.AssignedDriver)
            .Where(c => c.CarId == id)
            .Select(c => new CarDetailDto
            {
                CarId = c.CarId,
                CarName = c.CarName,
                Brand = c.Brand,
                CarType = c.CarType!.TypeName,
                PricePerDay = c.PricePerDay,
                Seats = c.Seats,
                Transmission = c.Transmission,
                FuelType = c.FuelType,
                Rating = c.Rating,
                ImageUrl = c.ImageUrl,
                IsAvailable = c.IsAvailable,
                Location = c.Location!.CityName,
                DriverName = c.AssignedDriver != null ? c.AssignedDriver.DriverName : string.Empty
            })
            .FirstOrDefaultAsync();

        if (car is null) return NotFound();

        return Ok(car);
    }
}
