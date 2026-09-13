using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Admin;

// Full car record as shown/edited on the admin "Manage Cars" screens.
public class AdminCarDto
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int CarTypeId { get; set; }
    public string CarType { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int Seats { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int LocationId { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? AssignedDriverId { get; set; }
    public string? DriverName { get; set; }
}

// Body for both add-car and edit-car - same shape either way.
public class SaveCarDto
{
    [Required, MaxLength(100)]
    public string CarName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public int CarTypeId { get; set; }

    [Range(0, 1000000)]
    public decimal PricePerDay { get; set; }

    [Range(1, 20)]
    public int Seats { get; set; }

    [Required, MaxLength(20)]
    public string Transmission { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string FuelType { get; set; } = string.Empty;

    [Range(0, 5)]
    public decimal Rating { get; set; }

    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [Required]
    public int LocationId { get; set; }

    // Null = no driver assigned yet.
    public int? AssignedDriverId { get; set; }
}
