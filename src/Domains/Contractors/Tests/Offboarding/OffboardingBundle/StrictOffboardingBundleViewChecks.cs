namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>Checks of offboarding bundle code, run by the project's runner.</summary>
public static class StrictOffboardingBundleViewChecks
{
    public static void StrictOffboardingBundleViewShowsTheStatusInCapitals()
    {
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("code-565", "label-572");
        var view = StrictOffboardingBundleView.ToStrictOffboardingBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictOffboardingBundleView.ToStrictOffboardingBundleViews(new List<StrictOffboardingBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictOffboardingBundleViewShowsTheStatusInCapitals();
    }
}
