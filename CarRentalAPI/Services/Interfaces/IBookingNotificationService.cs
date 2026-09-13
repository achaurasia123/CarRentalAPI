using CarRentalAPI.Entities;

namespace CarRentalAPI.Services.Interfaces;

// Fires the booking-confirmation SMS + email: one copy to the customer,
// one to every active Admin (so whoever is on duty sees who booked what).
// Called right after a booking is saved, from both CreateBooking and
// CreateDetailedBooking in BookingsController.
public interface IBookingNotificationService
{
    Task NotifyBookingConfirmedAsync(TrnBooking booking, MstUser customer, MstCar car, TrnBookingDetail? detail);
}
