namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTargetViewChecks
{
    public static void OffboardingTargetViewShowsTheStatusInCapitals()
    {
        var item = OffboardingTarget.NewOffboardingTarget("code-764", "title-35");
        var view = OffboardingTargetView.ToOffboardingTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingTargetView.ToOffboardingTargetViews(new List<OffboardingTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingTargetViewShowsTheStatusInCapitals();
    }
}
