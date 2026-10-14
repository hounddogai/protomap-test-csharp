namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRollupViewChecks
{
    public static void OffboardingRollupViewShowsTheStatusInCapitals()
    {
        var item = OffboardingRollup.NewOffboardingRollup("code-301", "label-554");
        var view = OffboardingRollupView.ToOffboardingRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingRollupView.ToOffboardingRollupViews(new List<OffboardingRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingRollupViewShowsTheStatusInCapitals();
    }
}
