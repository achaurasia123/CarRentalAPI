using System.Text;
using CarRentalAPI.Data;
using CarRentalAPI.Entities;
using CarRentalAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarRentalAPI.Services.Implementations;

public class BookingNotificationService : IBookingNotificationService
{
    private readonly AppDbContext _db;
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingNotificationService> _logger;

    public BookingNotificationService(
        AppDbContext db,
        ISmsSender smsSender,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<BookingNotificationService> logger)
    {
        _db = db;
        _smsSender = smsSender;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyBookingConfirmedAsync(TrnBooking booking, MstUser customer, MstCar car, TrnBookingDetail? detail)
    {
        var whenText = detail is not null
            ? $"{detail.BookingDate:dd MMM yyyy} at {detail.BookingTime}"
            : $"{booking.FromDate:dd MMM yyyy} to {booking.ToDate:dd MMM yyyy}";

        var routeText = detail is not null
            ? $"Route: {detail.FromLocationName} -> {detail.ToLocationName} ({detail.DistanceKm} km)"
            : null;

        // ---------- Customer copy ----------

        // Order must match SmsTemplateKind.CustomerBookingConfirmed's
        // documented {#var#} order: CustomerName, BookingNo, CarName, When, Amount.
        var customerSmsVars = new List<string>
        {
            customer.FullName,
            booking.BookingNo,
            car.CarName,
            whenText,
            booking.TotalAmount.ToString("N0")
        };

        var customerEmailBody = new StringBuilder()
            .AppendLine($"Hi {customer.FullName},")
            .AppendLine()
            .AppendLine("Your DriveOn booking is confirmed!")
            .AppendLine()
            .AppendLine($"Booking No: {booking.BookingNo}")
            .AppendLine($"Car: {car.CarName} ({car.Brand})")
            .AppendLine($"When: {whenText}")
            .AppendLineIfNotNull(routeText)
            .AppendLine($"Amount: Rs {booking.TotalAmount:N0}")
            .AppendLine()
            .AppendLine("Thanks for booking with DriveOn!")
            .ToString();

        try
        {
            await _smsSender.SendAsync(customer.PhoneNumber, SmsTemplateKind.CustomerBookingConfirmed, customerSmsVars);
            await _emailSender.SendAsync(customer.Email, $"Booking Confirmed - {booking.BookingNo}", customerEmailBody);
        }
        catch (Exception ex)
        {
            // Notifications must never take the booking transaction down with them.
            _logger.LogWarning(ex, "Failed sending customer notification for booking {BookingNo}.", booking.BookingNo);
        }

        // ---------- Admin copies ----------

        var adminNotificationsEnabled = _configuration.GetValue<bool?>("Notifications:AdminNotificationsEnabled") ?? true;
        if (!adminNotificationsEnabled)
        {
            return;
        }

        var admins = await _db.MstUsers
            .Where(u => u.RoleId == 1 && u.IsActive)
            .ToListAsync();

        if (admins.Count == 0)
        {
            return;
        }

        // Order must match SmsTemplateKind.AdminNewBooking's documented
        // {#var#} order: BookingNo, CustomerName, CarName, When.
        var adminSmsVars = new List<string>
        {
            booking.BookingNo,
            customer.FullName,
            car.CarName,
            whenText
        };

        var adminEmailBody = new StringBuilder()
            .AppendLine("A new booking has just been confirmed on DriveOn.")
            .AppendLine()
            .AppendLine($"Booking No: {booking.BookingNo}")
            .AppendLine($"Customer: {customer.FullName} ({customer.Email}, {customer.PhoneNumber})")
            .AppendLine($"Car: {car.CarName} ({car.Brand})")
            .AppendLine($"When: {whenText}")
            .AppendLineIfNotNull(routeText)
            .AppendLine($"Amount: Rs {booking.TotalAmount:N0}")
            .ToString();

        foreach (var admin in admins)
        {
            try
            {
                await _smsSender.SendAsync(admin.PhoneNumber, SmsTemplateKind.AdminNewBooking, adminSmsVars);
                await _emailSender.SendAsync(admin.Email, $"New Booking - {booking.BookingNo}", adminEmailBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed sending admin notification to {Email} for booking {BookingNo}.", admin.Email, booking.BookingNo);
            }
        }
    }
}

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendLineIfNotNull(this StringBuilder sb, string? line)
    {
        return string.IsNullOrWhiteSpace(line) ? sb : sb.AppendLine(line);
    }
}
