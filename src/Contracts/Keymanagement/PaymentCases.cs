namespace Acme.Keymanagement.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class CryptoKey
{
    public FundingOneofCase PaymentCase => FundingCase;
}
