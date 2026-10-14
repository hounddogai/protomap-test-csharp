namespace Acme.Licensing.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class LicenseAssignment
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class SoftwareLicense
{
    public FundingOneofCase PaymentCase => FundingCase;
}
