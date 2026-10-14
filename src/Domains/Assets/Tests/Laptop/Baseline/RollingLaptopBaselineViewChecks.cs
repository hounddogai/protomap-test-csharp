namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RollingLaptopBaselineViewChecks
{
    public static void RollingLaptopBaselineViewShowsTheStatusInCapitals()
    {
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("code-124", "name-970");
        var view = RollingLaptopBaselineView.ToRollingLaptopBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingLaptopBaselineView.ToRollingLaptopBaselineViews(new List<RollingLaptopBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingLaptopBaselineViewShowsTheStatusInCapitals();
    }
}
