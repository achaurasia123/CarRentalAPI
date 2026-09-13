using System.Globalization;
using System.Security.Claims;
using CarRentalAPI.Common;
using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Bookings;
using CarRentalAPI.Entities;
using CarRentalAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IBookingNotificationService _notificationService;

    public BookingsController(AppDbContext db, IBookingNotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "0");

    // GET /api/bookings  -> logged-in user's bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings()
    {
        var bookings = await _db.TrnBookings
            .Include(b => b.Car)
            .Include(b => b.Detail)
            .Where(b => b.UserId == CurrentUserId)
            .OrderByDescending(b => b.CreatedDate)
            .Select(b => new BookingDto
            {
                BookingId = b.BookingId,
                BookingNo = b.BookingNo,
                CarId = b.CarId,
                CarName = b.Car!.CarName,
                CarImage = b.Car.ImageUrl,
                FromDate = b.FromDate,
                ToDate = b.ToDate,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                CreatedDate = b.CreatedDate,
                FromLocation = b.Detail != null ? b.Detail.FromLocationName : null,
                ToLocation = b.Detail != null ? b.Detail.ToLocationName : null,
                DistanceKm = b.Detail != null ? b.Detail.DistanceKm : (decimal?)null,
                DriverName = b.Detail != null ? b.Detail.DriverName : null,
                BookingTime = b.Detail != null ? b.Detail.BookingTime : null
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // POST /api/bookings
    [HttpPost]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.ToDate <= request.FromDate)
        {
            return BadRequest(new { message = "To date must be after from date." });
        }

        var car = await _db.MstCars.FindAsync(request.CarId);
        if (car is null) return NotFound(new { message = "Car not found." });
        if (!car.IsAvailable) return Conflict(new { message = $"{car.CarName} is currently not available." });

        var days = Math.Max(1, (request.ToDate.Date - request.FromDate.Date).Days);
        var totalAmount = days * car.PricePerDay;

        var booking = new TrnBooking
        {
            BookingNo = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserId = CurrentUserId,
            CarId = car.CarId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TotalAmount = totalAmount,
            Status = "Confirmed",
            CreatedDate = DateTime.UtcNow
        };

        _db.TrnBookings.Add(booking);

        // Mark car unavailable while booked
        car.IsAvailable = false;

        await _db.SaveChangesAsync();

        var customer = await _db.MstUsers.FindAsync(CurrentUserId);
        if (customer is not null)
        {
            await _notificationService.NotifyBookingConfirmedAsync(booking, customer, car, detail: null);
        }

        return CreatedAtAction(nameof(GetMyBookings), new BookingDto
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo,
            CarId = car.CarId,
            CarName = car.CarName,
            CarImage = car.ImageUrl,
            FromDate = booking.FromDate,
            ToDate = booking.ToDate,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            CreatedDate = booking.CreatedDate
        });
    }

    // POST /api/bookings/detailed
    // The "Book Now" detail page's submit action: picks up car + fixed driver +
    // from/to location, works out the distance-based fare, and saves both the
    // usual trn_booking row (so My Bookings / cancel keep working unchanged)
    // and a trn_booking_detail row with the full snapshot of everything shown
    // on that page.
    [HttpPost("detailed")]
    public async Task<ActionResult<BookingDto>> CreateDetailedBooking([FromBody] CreateDetailedBookingDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var car = await _db.MstCars
            .Include(c => c.AssignedDriver)
            .FirstOrDefaultAsync(c => c.CarId == request.CarId);
        if (car is null) return NotFound(new { message = "Car not found." });
        if (!car.IsAvailable) return Conflict(new { message = $"{car.CarName} is currently not available." });

        var fromLocation = await _db.MstLocations.FindAsync(request.FromLocationId);
        var toLocation = await _db.MstLocations.FindAsync(request.ToLocationId);
        if (fromLocation is null || toLocation is null)
        {
            return BadRequest(new { message = "Please choose a valid from and to location." });
        }

        decimal distanceKm;
        if (request.FromLocationId == request.ToLocationId)
        {
            distanceKm = 0m;
        }
        else
        {
            var distanceRow = await _db.MstLocationDistances.FirstOrDefaultAsync(d =>
                d.FromLocationId == request.FromLocationId && d.ToLocationId == request.ToLocationId);

            if (distanceRow is null)
            {
                return BadRequest(new { message = $"No distance is configured between {fromLocation.CityName} and {toLocation.CityName} yet." });
            }

            distanceKm = distanceRow.DistanceKm;
        }

        if (!TimeSpan.TryParseExact(request.BookingTime, "hh\\:mm", CultureInfo.InvariantCulture, out var bookingTimeOfDay))
        {
            return BadRequest(new { message = "Booking time must be in HH:mm format." });
        }

        var amount = FarePricing.CalculateAmount(distanceKm);
        var pickupDateTime = request.BookingDate.Date + bookingTimeOfDay;

        var booking = new TrnBooking
        {
            BookingNo = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserId = CurrentUserId,
            CarId = car.CarId,
            FromDate = pickupDateTime,
            ToDate = pickupDateTime,
            TotalAmount = amount,
            Status = "Confirmed",
            CreatedDate = DateTime.UtcNow
        };

        _db.TrnBookings.Add(booking);
        car.IsAvailable = false;

        var driverName = car.AssignedDriver?.DriverName ?? string.Empty;

        var detail = new TrnBookingDetail
        {
            Booking = booking,
            CarId = car.CarId,
            CarName = car.CarName,
            CarBrand = car.Brand,
            FromLocationId = fromLocation.LocationId,
            FromLocationName = fromLocation.CityName,
            ToLocationId = toLocation.LocationId,
            ToLocationName = toLocation.CityName,
            DistanceKm = distanceKm,
            DriverId = car.AssignedDriverId,
            DriverName = driverName,
            BookingDate = request.BookingDate.Date,
            BookingTime = request.BookingTime,
            Amount = amount,
            CreatedDate = DateTime.UtcNow
        };

        _db.TrnBookingDetails.Add(detail);

        await _db.SaveChangesAsync();

        var customer = await _db.MstUsers.FindAsync(CurrentUserId);
        if (customer is not null)
        {
            await _notificationService.NotifyBookingConfirmedAsync(booking, customer, car, detail);
        }

        return CreatedAtAction(nameof(GetMyBookings), new BookingDto
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo,
            CarId = car.CarId,
            CarName = car.CarName,
            CarImage = car.ImageUrl,
            FromDate = booking.FromDate,
            ToDate = booking.ToDate,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            CreatedDate = booking.CreatedDate,
            FromLocation = fromLocation.CityName,
            ToLocation = toLocation.CityName,
            DistanceKm = distanceKm,
            DriverName = driverName,
            BookingTime = request.BookingTime
        });
    }

    // PUT /api/bookings/5/cancel
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _db.TrnBookings.Include(b => b.Car)
            .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == CurrentUserId);

        if (booking is null) return NotFound();

        booking.Status = "Cancelled";
        if (booking.Car is not null) booking.Car.IsAvailable = true;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
