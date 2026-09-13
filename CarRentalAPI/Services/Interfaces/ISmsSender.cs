namespace CarRentalAPI.Services.Interfaces;

// Thin wrapper so the SMS gateway can be swapped without touching call
// sites. Takes a template kind + ordered variable values rather than a
// free-text message, because Indian gateways (MSG91 included) only deliver
// text that matches a DLT-registered template - see SmsTemplateKind for the
// exact template wording each kind expects. Implementations must never
// throw for a missing/unconfigured gateway - just log and no-op - so
// booking creation itself never fails because a notification couldn't go
// out.
public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, SmsTemplateKind template, IReadOnlyList<string> variables);
}
