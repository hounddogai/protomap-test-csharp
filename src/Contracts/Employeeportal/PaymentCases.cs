namespace Acme.Employeeportal.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Announcement
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
