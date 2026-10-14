namespace Acme.Travel.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Itinerary
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class TravelBooking
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Trip
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
