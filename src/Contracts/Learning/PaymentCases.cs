namespace Acme.Learning.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class Certification
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class CourseEnrollment
{
    public FundingOneofCase PaymentCase => FundingCase;
}
