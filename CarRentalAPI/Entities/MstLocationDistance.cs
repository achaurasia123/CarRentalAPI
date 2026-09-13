using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

// Distance (in km) between a from-location and a to-location. Used to work out
// the location-wise fare on the detailed booking page: flat Rs.3500 up to 250km,
// then Rs.18/km beyond that. Seeded for every location pair; add rows here (or
// via SQL) if new cities/locations are added later.
[Table("mst_location_distance")]
public class MstLocationDistance
{
    [Key]
    [Column("distance_id")]
    public int DistanceId { get; set; }

    [Column("from_location_id")]
    public int FromLocationId { get; set; }

    [ForeignKey(nameof(FromLocationId))]
    public MstLocation? FromLocation { get; set; }

    [Column("to_location_id")]
    public int ToLocationId { get; set; }

    [ForeignKey(nameof(ToLocationId))]
    public MstLocation? ToLocation { get; set; }

    [Column("distance_km", TypeName = "decimal(8,2)")]
    public decimal DistanceKm { get; set; }
}
