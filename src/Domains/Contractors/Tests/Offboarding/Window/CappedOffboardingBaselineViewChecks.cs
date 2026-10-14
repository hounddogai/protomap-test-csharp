namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedOffboardingBaselineViewChecks
{
    public static void CappedOffboardingBaselineViewShowsTheStatusInCapitals()
    {
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("code-502", "title-116");
        var view = CappedOffboardingBaselineView.ToCappedOffboardingBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedOffboardingBaselineView.ToCappedOffboardingBaselineViews(new List<CappedOffboardingBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedOffboardingBaselineViewShowsTheStatusInCapitals();
    }
}
