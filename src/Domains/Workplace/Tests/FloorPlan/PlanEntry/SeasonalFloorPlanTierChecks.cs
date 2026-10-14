namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Checks of plan entry code, run by the project's runner.</summary>
public static class SeasonalFloorPlanTierChecks
{
    public static void SeasonalFloorPlanTierStartsInDraft()
    {
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("code-88", "title-348");
        Check.Equal(item.Status, SeasonalFloorPlanTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFloorPlanTierMovesToActive()
    {
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("code-88", "title-348");
        try
        {
            item.MoveTo(SeasonalFloorPlanTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFloorPlanTierStatus.Active, "status");
    }

    public static void SeasonalFloorPlanTierRejectsMovesFromClosed()
    {
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("code-88", "title-348");
        item.Status = SeasonalFloorPlanTierStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalFloorPlanTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanTierStartsInDraft();
        SeasonalFloorPlanTierMovesToActive();
        SeasonalFloorPlanTierRejectsMovesFromClosed();
    }
}
