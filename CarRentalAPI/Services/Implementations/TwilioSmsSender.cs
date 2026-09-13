using System.Net.Http.Headers;
using CarRentalAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarRentalAPI.Services.Implementations;

// Calls Twilio's REST API directly over HttpClient (no Twilio NuGet package
// needed - one POST with Basic Auth is all "send an SMS" requires). Reads
// credentials from configuration ("Sms:Twilio:..."); if they're blank (not
// set up yet) this just logs a warning and returns instead of throwing, so
// booking creation is never blocked on SMS being configured.
//
// NOT registered in Program.cs right now - Msg91SmsSender is (per the
// project's choice of MSG91 as the SMS gateway). Kept here, working and
// up to date with ISmsSender, in case the project ever wants to switch
// gateways: just swap the DI registration in Program.cs. Twilio has no
// DLT/template restriction the way Indian gateways do, so this formats
// each SmsTemplateKind's variables into plain text itself.
public class TwilioSmsSender : ISmsSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TwilioSmsSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string toPhoneNumber, SmsTemplateKind template, IReadOnlyList<string> variables)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            return; // No phone on file for this user - nothing to do.
        }

        var message = FormatMessage(template, variables);

        var accountSid = _configuration["Sms:Twilio:AccountSid"];
        var authToken = _configuration["Sms:Twilio:AuthToken"];
        var fromNumber = _configuration["Sms:Twilio:FromNumber"];

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(fromNumber))
        {
            _logger.LogWarning(
                "Twilio is not configured (Sms:Twilio:AccountSid/AuthToken/FromNumber in appsettings) - skipping SMS to {Phone}.",
                toPhoneNumber);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{accountSid}:{authToken}")));

            var form = new Dictionary<string, string>
            {
                ["To"] = NormalizeToE164(toPhoneNumber),
                ["From"] = fromNumber,
                ["Body"] = message
            };

            var response = await client.PostAsync(
                $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json",
                new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Twilio SMS to {Phone} failed ({Status}): {Body}", toPhoneNumber, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Never let a notification failure break the booking flow.
            _logger.LogWarning(ex, "Twilio SMS to {Phone} threw an exception.", toPhoneNumber);
        }
    }

    // Mirrors the wording documented on SmsTemplateKind, just without the
    // {#var#} DLT template mechanics - Twilio just wants a plain string.
    private static string FormatMessage(SmsTemplateKind template, IReadOnlyList<string> variables)
    {
        return template switch
        {
            SmsTemplateKind.CustomerBookingConfirmed =>
                $"Dear {variables[0]}, your DriveOn booking {variables[1]} for {variables[2]} on {variables[3]} is CONFIRMED. Amount: Rs {variables[4]}. Thank you - DriveOn",
            SmsTemplateKind.AdminNewBooking =>
                $"DriveOn Alert: New booking {variables[0]} by {variables[1]} for {variables[2]} on {variables[3]}. Check admin dashboard for details.",
            _ => string.Join(" ", variables)
        };
    }

    // Twilio needs E.164 (e.g. +919800000001). Numbers are stored as plain
    // 10-digit Indian mobile numbers, so default to the +91 country code
    // unless the value already looks like it has one.
    private static string NormalizeToE164(string phoneNumber)
    {
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (phoneNumber.TrimStart().StartsWith("+"))
        {
            return "+" + digitsOnly;
        }

        return digitsOnly.Length == 10 ? $"+91{digitsOnly}" : $"+{digitsOnly}";
    }
}
