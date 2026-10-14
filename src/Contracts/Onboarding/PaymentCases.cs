namespace Acme.Onboarding.V1;

/// <summary>
/// Keeps the payments adapter independent of the legacy funding or settlement name.
/// </summary>
public sealed partial class OnboardingPlan
{
    public SettlementOneofCase PaymentCase => SettlementCase;
}
