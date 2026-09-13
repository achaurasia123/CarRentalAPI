using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using CarRentalAPI.Dtos.Common;
using CarRentalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Fleet-wide bookings view for the admin "Manage Bookings" screen -
// BookingsController.GetMyBookings only ever returns the signed-in user's
// own bookings; this is every customer's bookings, filterable and paged.
[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin")]
public class AdminBookingsController : ControllerBase
{
    // A booking can only move on from an "active" state - once Cancelled or
    // Completed it's final and can never be reopened or re-cancelled from here.
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        ["Pending"] = new[] { "Confirmed", "Cancelled" },
        ["Confirmed"] = new[] { "Completed", "Cancelled" }
    };

    private readonly AppDbContext _db;

    public AdminBookingsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/bookings?status=Confirmed&carId=3&customerSearch=demo&fromDate=2026-01-01&toDate=2026-12-31&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AdminBookingDto>>> GetBookings([FromQuery] AdminBookingQueryDto query)
    {
        var bookings = _db.TrnBookings
            .Include(b => b.User)
            .Include(b => b.Car)
            .Include(b => b.Detail)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            bookings = bookings.Where(b => b.Status == query.Status);
        }

        if (query.CarId.HasValue)
        {
            bookings = bookings.Where(b => b.CarId == query.CarId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CustomerSearch))
        {
            var term = query.CustomerSearch.Trim();
            bookings = bookings.Where(b =>
                b.User != null && (b.User.FullName.Contains(term) || b.User.Email.Contains(term)));
        }

        if (query.FromDate.HasValue)
        {
            bookings = bookings.Where(b => b.FromDate.Date >= query.FromDate.Value.Date);
        }

        if (query.ToDate.HasValue)
        {
            bookings = bookings.Where(b => b.ToDate.Date <= query.ToDate.Value.Date);
        }

        bookings = bookings.OrderByDescending(b => b.CreatedDate);

        var totalCount = await bookings.CountAsync();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        // Materialize the page first, THEN map to DTOs in memory - EF Core
        // can't translate a C# mapping method into SQL inside .Select().
        var pageEntities = await bookings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResultDto<AdminBookingDto>
        {
            Items = pageEntities.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // PUT /api/admin/bookings/5/status
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<AdminBookingDto>> UpdateStatus(int id, [FromBody] UpdateBookingStatusDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var booking = await _db.TrnBookings
            .Include(b => b.User)
            .Include(b => b.Car)
            .Include(b => b.Detail)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking is null) return NotFound();

        if (!AllowedTransitions.TryGetValue(booking.Status, out var allowedNext) ||
            !allowedNext.Contains(request.Status))
        {
            return Conflict(new
            {
                message = $"Booking {booking.BookingNo} is already {booking.Status} and can't be moved to {request.Status}."
            });
        }

        booking.Status = request.Status;

        // Both Completed (trip finished, car returned) and Cancelled free the
        // car back up - mirrors BookingsController.CancelBooking's behaviour.
        if ((request.Status == "Completed" || request.Status == "Cancelled") && booking.Car is not null)
        {
            booking.Car.IsAvailable = true;
        }

        await _db.SaveChangesAsync();

        return Ok(ToDto(booking));
    }

    private static AdminBookingDto ToDto(TrnBooking b) => new()
    {
        BookingId = b.BookingId,
        BookingNo = b.BookingNo,
        UserId = b.UserId,
        CustomerName = b.User?.FullName ?? string.Empty,
        CustomerEmail = b.User?.Email ?? string.Empty,
        CarId = b.CarId,
        CarName = b.Car?.CarName ?? string.Empty,
        CarImage = b.Car?.ImageUrl ?? string.Empty,
        FromDate = b.FromDate,
        ToDate = b.ToDate,
        TotalAmount = b.TotalAmount,
        Status = b.Status,
        CreatedDate = b.CreatedDate,
        FromLocation = b.Detail?.FromLocationName,
        ToLocation = b.Detail?.ToLocationName,
        DistanceKm = b.Detail?.DistanceKm,
        DriverName = b.Detail?.DriverName,
        BookingTime = b.Detail?.BookingTime
    };
}
