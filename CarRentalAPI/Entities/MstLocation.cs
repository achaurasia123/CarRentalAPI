using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

[Table("mst_location")]
public class MstLocation
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("city_name")]
    [MaxLength(100)]
    public string CityName { get; set; } = string.Empty;

    public ICollection<MstCar> Cars { get; set; } = new List<MstCar>();
}
