using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalAPI.Entities;

[Table("mst_user")]
public class MstUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("full_name")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // Captured at registration so booking-confirmation SMS can reach the
    // customer; existing rows (seeded before this field existed) default to
    // empty and just skip SMS until the person fills it in.
    [Column("phone_number")]
    [MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("password_hash")]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("role_id")]
    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public MstRole? Role { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<TrnBooking> Bookings { get; set; } = new List<TrnBooking>();
    public ICollection<TblRefreshToken> RefreshTokens { get; set; } = new List<TblRefreshToken>();
}
