namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRollupViewChecks
{
    public static void PrimarySuccessionPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("code-697", "label-208");
        var view = PrimarySuccessionPlanRollupView.ToPrimarySuccessionPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimarySuccessionPlanRollupView.ToPrimarySuccessionPlanRollupViews(new List<PrimarySuccessionPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimarySuccessionPlanRollupViewShowsTheStatusInCapitals();
    }
}
