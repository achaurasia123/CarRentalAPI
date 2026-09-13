using CarRentalAPI.Entities;

namespace CarRentalAPI.Services.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateJwtToken(MstUser user);
}
