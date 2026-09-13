namespace CarRentalAPI.Services.Interfaces;

// Same no-throw contract as ISmsSender - a missing/unconfigured SMTP
// account should never fail the booking request that triggered the email.
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}
