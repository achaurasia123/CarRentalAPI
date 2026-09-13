using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Admin;

// One row in the admin "Manage Bookings" table - every booking across every
// customer, unlike BookingsController.GetMyBookings which only returns the
// signed-in user's own bookings.
public class AdminBookingDto
{
    public int BookingId { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string CarImage { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Only set for bookings made through the detailed "Book Now" flow.
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public decimal? DistanceKm { get; set; }
    public string? DriverName { get; set; }
    public string? BookingTime { get; set; }
}

// GET /api/admin/bookings query filters - everything optional.
public class AdminBookingQueryDto
{
    public string? Status { get; set; }
    public int? CarId { get; set; }

    // Matches against customer full name or email (case-insensitive contains).
    public string? CustomerSearch { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Deliberately unvalidated (no [Range]) - [ApiController] auto-400s on an
    // invalid ModelState, which would turn a stray page=0 into a hard error
    // instead of the graceful clamp AdminBookingsController applies.
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

// PUT /api/admin/bookings/{id}/status - the only two admin-initiated
// transitions are marking a trip done or cancelling it.
public class UpdateBookingStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // "Completed" or "Cancelled"
}
