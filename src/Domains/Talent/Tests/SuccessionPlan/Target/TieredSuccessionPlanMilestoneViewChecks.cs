namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredSuccessionPlanMilestoneViewChecks
{
    public static void TieredSuccessionPlanMilestoneViewShowsTheStatusInCapitals()
    {
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("code-896", "name-883");
        var view = TieredSuccessionPlanMilestoneView.ToTieredSuccessionPlanMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredSuccessionPlanMilestoneView.ToTieredSuccessionPlanMilestoneViews(new List<TieredSuccessionPlanMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredSuccessionPlanMilestoneViewShowsTheStatusInCapitals();
    }
}
