using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

[Table("mst_car_type")]
public class MstCarType
{
    [Key]
    [Column("car_type_id")]
    public int CarTypeId { get; set; }

    [Column("type_name")]
    [MaxLength(50)]
    public string TypeName { get; set; } = string.Empty;

    public ICollection<MstCar> Cars { get; set; } = new List<MstCar>();
}
