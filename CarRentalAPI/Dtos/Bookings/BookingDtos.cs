using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Bookings;

public class CreateBookingDto
{
    [Required]
    public int CarId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}

public class BookingDto
{
    public int BookingId { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string CarImage { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Only set for bookings made through the detailed "Book Now" flow (from/to
    // location + fixed driver + distance-based fare). Null for older/instant
    // bookings that have no trn_booking_detail row.
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public decimal? DistanceKm { get; set; }
    public string? DriverName { get; set; }
    public string? BookingTime { get; set; }
}

// POST /api/bookings/detailed - the "Book Now" detail page's submit payload.
public class CreateDetailedBookingDto
{
    [Required]
    public int CarId { get; set; }

    [Required]
    public int FromLocationId { get; set; }

    [Required]
    public int ToLocationId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    [MaxLength(10)]
    public string BookingTime { get; set; } = string.Empty; // "HH:mm"
}
