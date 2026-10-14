namespace Acme.Scheduling.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Roster
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
