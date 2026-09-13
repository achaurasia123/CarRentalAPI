using System.Text;
using System.Text.Json;
using CarRentalAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarRentalAPI.Services.Implementations;

// Calls MSG91's Flow API (https://control.msg91.com/api/v5/flow/) directly
// over HttpClient - no MSG91 NuGet/SDK package needed. Flow API sends a
// DLT-approved template filled in with VAR1, VAR2, ... values (in the same
// order the template's {#var#} placeholders appear) - see SmsTemplateKind
// for the exact wording each template needs to be registered with on
// MSG91's DLT/template dashboard.
//
// IMPORTANT: this project's DevOps could not independently re-verify MSG91's
// current exact field names against their live docs (their docs site is a
// JS app that didn't render for automated fetching). This follows MSG91's
// long-standing, widely-documented Flow API contract, but when you create
// your template on the MSG91 dashboard, MSG91 shows an exact copy-paste
// code sample for YOUR account/template - cross-check field names
// (template_id / recipients / mobiles / VAR1..) against that sample and
// adjust BuildRequestBody below if anything differs.
public class Msg91SmsSender : ISmsSender
{
    private const string FlowApiUrl = "https://control.msg91.com/api/v5/flow/";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Msg91SmsSender> _logger;

    public Msg91SmsSender(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Msg91SmsSender> logger)
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

        var authKey = _configuration["Sms:Msg91:AuthKey"];
        var senderId = _configuration["Sms:Msg91:SenderId"];
        var templateId = _configuration[$"Sms:Msg91:{template}TemplateId"];

        if (string.IsNullOrWhiteSpace(authKey) || string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(templateId))
        {
            _logger.LogWarning(
                "MSG91 is not fully configured (Sms:Msg91:AuthKey/SenderId/{Template}TemplateId in appsettings) - skipping SMS to {Phone}.",
                template, toPhoneNumber);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("authkey", authKey);

            var requestBody = BuildRequestBody(templateId, senderId, toPhoneNumber, variables);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(FlowApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("MSG91 SMS to {Phone} failed ({Status}): {Body}", toPhoneNumber, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Never let a notification failure break the booking flow.
            _logger.LogWarning(ex, "MSG91 SMS to {Phone} threw an exception.", toPhoneNumber);
        }
    }

    private static string BuildRequestBody(string templateId, string senderId, string toPhoneNumber, IReadOnlyList<string> variables)
    {
        var recipient = new Dictionary<string, string>
        {
            ["mobiles"] = NormalizeToMsg91Format(toPhoneNumber)
        };

        for (var i = 0; i < variables.Count; i++)
        {
            recipient[$"VAR{i + 1}"] = variables[i];
        }

        var payload = new Dictionary<string, object>
        {
            ["template_id"] = templateId,
            ["sender"] = senderId,
            ["short_url"] = "0",
            ["recipients"] = new[] { recipient }
        };

        return JsonSerializer.Serialize(payload);
    }

    // MSG91 expects the number with country code, no "+" (e.g. 919800000001).
    // Numbers are stored as plain 10-digit Indian mobile numbers, so default
    // to the 91 country code unless the value already has one.
    private static string NormalizeToMsg91Format(string phoneNumber)
    {
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digitsOnly.Length == 10 ? $"91{digitsOnly}" : digitsOnly;
    }
}
