namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class StrictOffboardingOverrideViewChecks
{
    public static void StrictOffboardingOverrideViewShowsTheStatusInCapitals()
    {
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("code-553", "title-73");
        var view = StrictOffboardingOverrideView.ToStrictOffboardingOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictOffboardingOverrideView.ToStrictOffboardingOverrideViews(new List<StrictOffboardingOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictOffboardingOverrideViewShowsTheStatusInCapitals();
    }
}
