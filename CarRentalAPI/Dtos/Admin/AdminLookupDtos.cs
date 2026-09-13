namespace CarRentalAPI.Dtos.Admin;

// Small id+name pair used to populate the car-type dropdown on the admin
// add/edit car form (the existing /api/cars/types endpoint only returns
// type names, not ids, which the save request actually needs).
public class CarTypeOptionDto
{
    public int CarTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
