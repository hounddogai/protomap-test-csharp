namespace Acme.Budgeting.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Budget
{
    public FundingOneofCase PaymentCase => FundingCase;
}
