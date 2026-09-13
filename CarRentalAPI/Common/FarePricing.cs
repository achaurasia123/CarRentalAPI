namespace CarRentalAPI.Common;

// Shared pricing rule for the location-based "Book Now" flow: a flat fare
// covers the first stretch of the trip, then a per-km rate applies beyond
// that. Kept in one place so the live price preview (LocationsController)
// and the actual booking creation (BookingsController) can never drift apart.
public static class FarePricing
{
    public const decimal FlatFareUpToKm = 250m;
    public const decimal FlatFareAmount = 3500m;
    public const decimal PerKmBeyondFlat = 18m;

    public static decimal CalculateAmount(decimal distanceKm)
    {
        if (distanceKm <= FlatFareUpToKm) return FlatFareAmount;

        var extraKm = distanceKm - FlatFareUpToKm;
        return FlatFareAmount + extraKm * PerKmBeyondFlat;
    }
}
