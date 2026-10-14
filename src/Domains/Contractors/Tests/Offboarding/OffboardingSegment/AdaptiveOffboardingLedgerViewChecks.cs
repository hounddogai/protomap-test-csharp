namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingLedgerViewChecks
{
    public static void AdaptiveOffboardingLedgerViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("code-126", "name-867");
        var view = AdaptiveOffboardingLedgerView.ToAdaptiveOffboardingLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveOffboardingLedgerView.ToAdaptiveOffboardingLedgerViews(new List<AdaptiveOffboardingLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingLedgerViewShowsTheStatusInCapitals();
    }
}
