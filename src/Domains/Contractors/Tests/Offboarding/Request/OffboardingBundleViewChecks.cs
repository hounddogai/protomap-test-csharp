namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingBundleViewChecks
{
    public static void OffboardingBundleViewShowsTheStatusInCapitals()
    {
        var item = OffboardingBundle.NewOffboardingBundle("code-129", "name-918");
        var view = OffboardingBundleView.ToOffboardingBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingBundleView.ToOffboardingBundleViews(new List<OffboardingBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingBundleViewShowsTheStatusInCapitals();
    }
}
