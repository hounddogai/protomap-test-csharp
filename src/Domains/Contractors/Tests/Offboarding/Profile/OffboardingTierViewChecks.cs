namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTierViewChecks
{
    public static void OffboardingTierViewShowsTheStatusInCapitals()
    {
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
        var view = OffboardingTierView.ToOffboardingTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingTierView.ToOffboardingTierViews(new List<OffboardingTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingTierViewShowsTheStatusInCapitals();
    }
}
