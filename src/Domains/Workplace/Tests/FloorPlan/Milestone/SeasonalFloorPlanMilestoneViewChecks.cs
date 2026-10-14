namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalFloorPlanMilestoneViewChecks
{
    public static void SeasonalFloorPlanMilestoneViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("code-365", "label-47");
        var view = SeasonalFloorPlanMilestoneView.ToSeasonalFloorPlanMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFloorPlanMilestoneView.ToSeasonalFloorPlanMilestoneViews(new List<SeasonalFloorPlanMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanMilestoneViewShowsTheStatusInCapitals();
    }
}
