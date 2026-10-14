namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Checks of plan entry code, run by the project's runner.</summary>
public static class SeasonalFloorPlanTierViewChecks
{
    public static void SeasonalFloorPlanTierViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("code-88", "title-348");
        var view = SeasonalFloorPlanTierView.ToSeasonalFloorPlanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFloorPlanTierView.ToSeasonalFloorPlanTierViews(new List<SeasonalFloorPlanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanTierViewShowsTheStatusInCapitals();
    }
}
