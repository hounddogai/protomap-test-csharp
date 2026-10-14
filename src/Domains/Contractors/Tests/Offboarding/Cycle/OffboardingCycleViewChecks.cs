namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleViewChecks
{
    public static void OffboardingCycleViewShowsTheStatusInCapitals()
    {
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
        var view = OffboardingCycleView.ToOffboardingCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingCycleView.ToOffboardingCycleViews(new List<OffboardingCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingCycleViewShowsTheStatusInCapitals();
    }
}
