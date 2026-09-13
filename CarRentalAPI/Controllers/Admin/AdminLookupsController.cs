using CarRentalAPI.Data;
using CarRentalAPI.Dtos.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Controllers.Admin;

// Small helper lookups for populating dropdowns on the admin car form
// (locations and drivers already have their own list endpoints elsewhere).
[ApiController]
[Route("api/admin/lookups")]
[Authorize(Roles = "Admin")]
public class AdminLookupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminLookupsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/lookups/car-types
    [HttpGet("car-types")]
    public async Task<ActionResult<IEnumerable<CarTypeOptionDto>>> GetCarTypes()
    {
        var types = await _db.MstCarTypes
            .OrderBy(t => t.CarTypeId)
            .Select(t => new CarTypeOptionDto { CarTypeId = t.CarTypeId, TypeName = t.TypeName })
            .ToListAsync();

        return Ok(types);
    }
}
