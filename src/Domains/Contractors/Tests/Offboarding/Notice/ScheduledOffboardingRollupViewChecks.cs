namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledOffboardingRollupViewChecks
{
    public static void ScheduledOffboardingRollupViewShowsTheStatusInCapitals()
    {
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("code-999", "title-963");
        var view = ScheduledOffboardingRollupView.ToScheduledOffboardingRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledOffboardingRollupView.ToScheduledOffboardingRollupViews(new List<ScheduledOffboardingRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledOffboardingRollupViewShowsTheStatusInCapitals();
    }
}
