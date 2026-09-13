using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

// Full snapshot of everything shown/decided on the "Book Now" detail page for a
// single booking: date/time, from->to location, driver, car name/model and the
// final amount - saved as its own row (linked back to trn_booking) so the exact
// details of a booking are preserved even if the car/driver/location data
// changes later.
[Table("trn_booking_detail")]
public class TrnBookingDetail
{
    [Key]
    [Column("booking_detail_id")]
    public int BookingDetailId { get; set; }

    [Column("booking_id")]
    public int BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public TrnBooking? Booking { get; set; }

    [Column("car_id")]
    public int CarId { get; set; }

    [Column("car_name")]
    [MaxLength(100)]
    public string CarName { get; set; } = string.Empty;

    [Column("car_brand")]
    [MaxLength(100)]
    public string CarBrand { get; set; } = string.Empty;

    [Column("from_location_id")]
    public int FromLocationId { get; set; }

    [ForeignKey(nameof(FromLocationId))]
    public MstLocation? FromLocation { get; set; }

    [Column("from_location_name")]
    [MaxLength(100)]
    public string FromLocationName { get; set; } = string.Empty;

    [Column("to_location_id")]
    public int ToLocationId { get; set; }

    [ForeignKey(nameof(ToLocationId))]
    public MstLocation? ToLocation { get; set; }

    [Column("to_location_name")]
    [MaxLength(100)]
    public string ToLocationName { get; set; } = string.Empty;

    [Column("distance_km", TypeName = "decimal(8,2)")]
    public decimal DistanceKm { get; set; }

    [Column("driver_id")]
    public int? DriverId { get; set; }

    [ForeignKey(nameof(DriverId))]
    public MstDriver? Driver { get; set; }

    [Column("driver_name")]
    [MaxLength(100)]
    public string DriverName { get; set; } = string.Empty;

    [Column("booking_date")]
    public DateTime BookingDate { get; set; }

    [Column("booking_time")]
    [MaxLength(10)]
    public string BookingTime { get; set; } = string.Empty; // e.g. "14:30"

    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
