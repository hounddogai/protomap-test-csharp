namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PendingOffboardingCycleViewChecks
{
    public static void PendingOffboardingCycleViewShowsTheStatusInCapitals()
    {
        var item = PendingOffboardingCycle.NewPendingOffboardingCycle("code-981", "label-227");
        var view = PendingOffboardingCycleView.ToPendingOffboardingCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingOffboardingCycleView.ToPendingOffboardingCycleViews(new List<PendingOffboardingCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingOffboardingCycleViewShowsTheStatusInCapitals();
    }
}
