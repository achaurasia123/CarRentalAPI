using System.ComponentModel.DataAnnotations;

namespace CarRentalAPI.Dtos.Admin;

public class AdminUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int TotalBookings { get; set; }
}

// PUT /api/admin/users/{id}/status
public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

// PUT /api/admin/users/{id}/role
public class UpdateUserRoleDto
{
    [Required]
    public int RoleId { get; set; }
}

// PUT /api/admin/users/{id}/phone - lets an admin set/correct any account's
// contact number, including their own (needed so booking-confirmation
// notifications have somewhere to send the admin copy).
public class UpdateUserPhoneDto
{
    [Required]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
