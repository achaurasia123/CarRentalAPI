using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Admin;

public class AdminDriverDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // How many cars currently have this driver fixed/assigned - shown so the
    // admin knows why a driver can't be deleted, and which car(s) to check.
    public int AssignedCarsCount { get; set; }
    public string AssignedCarNames { get; set; } = string.Empty;
}

// Body for both add-driver and edit-driver.
public class SaveDriverDto
{
    [Required, MaxLength(100)]
    public string DriverName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
