namespace Acme.Recruiting.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class JobOpening
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Offer
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
