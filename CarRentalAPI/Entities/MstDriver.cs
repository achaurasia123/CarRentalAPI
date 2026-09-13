using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

// Master list of drivers DriveOn keeps on staff. Each car has one of these
// permanently/already assigned to it (see MstCar.AssignedDriverId) - customers
// don't pick a driver themselves, the car comes with its fixed driver.
[Table("mst_driver")]
public class MstDriver
{
    [Key]
    [Column("driver_id")]
    public int DriverId { get; set; }

    [Column("driver_name")]
    [MaxLength(100)]
    public string DriverName { get; set; } = string.Empty;

    [Column("phone_number")]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("license_number")]
    [MaxLength(30)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<MstCar> Cars { get; set; } = new List<MstCar>();
}
