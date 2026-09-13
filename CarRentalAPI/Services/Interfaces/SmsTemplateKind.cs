namespace CarRentalAPI.Services.Interfaces;

// Indian SMS gateways (MSG91 included) can only send text that matches a
// DLT-registered template - free-form text gets rejected by the telecom
// operator's DLT scrubbing, regardless of which gateway API you call. So
// instead of a single free-text message, callers pick a template "kind" and
// pass the ordered variable values that fill that template's {#var#} slots.
public enum SmsTemplateKind
{
    // "Dear {#var#}, your DriveOn booking {#var#} for {#var#} on {#var#} is
    // CONFIRMED. Amount: Rs {#var#}. Thank you - DriveOn"
    // Vars in order: CustomerName, BookingNo, CarName, When, Amount
    CustomerBookingConfirmed,

    // "DriveOn Alert: New booking {#var#} by {#var#} for {#var#} on {#var#}.
    // Check admin dashboard for details."
    // Vars in order: BookingNo, CustomerName, CarName, When
    AdminNewBooking
}
