using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

[Table("mst_car")]
public class MstCar
{
    [Key]
    [Column("car_id")]
    public int CarId { get; set; }

    [Column("car_name")]
    [MaxLength(100)]
    public string CarName { get; set; } = string.Empty;

    [Column("brand")]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Column("car_type_id")]
    public int CarTypeId { get; set; }

    [ForeignKey(nameof(CarTypeId))]
    public MstCarType? CarType { get; set; }

    [Column("price_per_day", TypeName = "decimal(10,2)")]
    public decimal PricePerDay { get; set; }

    [Column("seats")]
    public int Seats { get; set; }

    [Column("transmission")]
    [MaxLength(20)]
    public string Transmission { get; set; } = string.Empty; // Manual / Automatic

    [Column("fuel_type")]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty; // Petrol / Diesel / Electric / Hybrid

    [Column("rating", TypeName = "decimal(2,1)")]
    public decimal Rating { get; set; }

    [Column("image_url")]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [Column("location_id")]
    public int LocationId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public MstLocation? Location { get; set; }

    // The driver that already comes fixed/assigned with this car (drivers are
    // managed from mst_driver, not chosen by the customer per-booking).
    [Column("assigned_driver_id")]
    public int? AssignedDriverId { get; set; }

    [ForeignKey(nameof(AssignedDriverId))]
    public MstDriver? AssignedDriver { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<TrnBooking> Bookings { get; set; } = new List<TrnBooking>();
}
