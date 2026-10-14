namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanRollupViewChecks
{
    public static void SuccessionPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
        var view = SuccessionPlanRollupView.ToSuccessionPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanRollupView.ToSuccessionPlanRollupViews(new List<SuccessionPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanRollupViewShowsTheStatusInCapitals();
    }
}
