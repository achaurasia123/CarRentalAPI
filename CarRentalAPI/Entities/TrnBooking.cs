using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

[Table("trn_booking")]
public class TrnBooking
{
    [Key]
    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("booking_no")]
    [MaxLength(20)]
    public string BookingNo { get; set; } = string.Empty;

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public MstUser? User { get; set; }

    [Column("car_id")]
    public int CarId { get; set; }

    [ForeignKey(nameof(CarId))]
    public MstCar? Car { get; set; }

    [Column("from_date")]
    public DateTime FromDate { get; set; }

    [Column("to_date")]
    public DateTime ToDate { get; set; }

    [Column("total_amount", TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending / Confirmed / Cancelled / Completed

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Present only for bookings made through the detailed "Book Now" flow
    // (from/to location, driver, distance-based amount etc). Older/instant
    // bookings won't have one.
    public TrnBookingDetail? Detail { get; set; }
}
