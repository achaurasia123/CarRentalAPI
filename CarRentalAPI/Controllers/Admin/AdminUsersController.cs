using System.Security.Claims;
using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using CarRentalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Customer/admin account management for the admin "Manage Customers" screen.
// Deliberately no hard delete here - MstUser rows are referenced by
// TrnBookings (Restrict delete behaviour) and deleting a person's account
// shouldn't erase their booking history, so accounts are only ever
// activated/deactivated (soft-delete) or promoted/demoted between roles.
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "0");

    // GET /api/admin/users?search=demo&roleId=2&isActive=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers(
        [FromQuery] string? search, [FromQuery] int? roleId, [FromQuery] bool? isActive)
    {
        var query = _db.MstUsers.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.FullName.Contains(term) || u.Email.Contains(term));
        }

        if (roleId.HasValue)
        {
            query = query.Where(u => u.RoleId == roleId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync();
        var bookingCounts = await _db.TrnBookings
            .GroupBy(b => b.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var result = users.Select(u => new AdminUserDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            Role = u.Role?.RoleName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedDate = u.CreatedDate,
            TotalBookings = bookingCounts.TryGetValue(u.UserId, out var count) ? count : 0
        });

        return Ok(result);
    }

    // PUT /api/admin/users/5/phone - any account, including the admin's own
    // (no self/last-admin restriction here, it's just contact info).
    [HttpPut("{id:int}/phone")]
    public async Task<ActionResult<AdminUserDto>> UpdatePhone(int id, [FromBody] UpdateUserPhoneDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _db.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null) return NotFound();

        user.PhoneNumber = request.PhoneNumber;
        await _db.SaveChangesAsync();

        return Ok(await ToDtoAsync(user));
    }

    // PUT /api/admin/users/5/status
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateStatus(int id, [FromBody] UpdateUserStatusDto request)
    {
        var user = await _db.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null) return NotFound();

        if (!request.IsActive)
        {
            if (id == CurrentUserId)
            {
                return Conflict(new { message = "You can't deactivate your own account." });
            }

            if (await IsLastActiveAdminAsync(user))
            {
                return Conflict(new { message = "This is the last active Admin - deactivate another Admin first." });
            }
        }

        user.IsActive = request.IsActive;
        await _db.SaveChangesAsync();

        return Ok(await ToDtoAsync(user));
    }

    // PUT /api/admin/users/5/role
    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<AdminUserDto>> UpdateRole(int id, [FromBody] UpdateUserRoleDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _db.MstUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        if (user is null) return NotFound();

        var roleExists = await _db.MstRoles.AnyAsync(r => r.RoleId == request.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { message = "Please choose a valid role." });
        }

        if (request.RoleId == user.RoleId)
        {
            return Ok(await ToDtoAsync(user)); // no-op, nothing to change
        }

        if (id == CurrentUserId)
        {
            return Conflict(new { message = "You can't change your own role." });
        }

        if (user.RoleId == 1 && await IsLastActiveAdminAsync(user))
        {
            return Conflict(new { message = "This is the last active Admin - promote another user to Admin first." });
        }

        user.RoleId = request.RoleId;
        await _db.Entry(user).Reference(u => u.Role).LoadAsync();
        await _db.SaveChangesAsync();

        return Ok(await ToDtoAsync(user));
    }

    // True when `user` is currently an active Admin and no other active Admin exists.
    private async Task<bool> IsLastActiveAdminAsync(MstUser user)
    {
        if (user.RoleId != 1 || !user.IsActive) return false;

        var otherActiveAdmins = await _db.MstUsers
            .CountAsync(u => u.UserId != user.UserId && u.RoleId == 1 && u.IsActive);

        return otherActiveAdmins == 0;
    }

    private async Task<AdminUserDto> ToDtoAsync(MstUser u)
    {
        var totalBookings = await _db.TrnBookings.CountAsync(b => b.UserId == u.UserId);

        return new AdminUserDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            Role = u.Role?.RoleName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedDate = u.CreatedDate,
            TotalBookings = totalBookings
        };
    }
}
