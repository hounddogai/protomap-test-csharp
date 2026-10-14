namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalFloorPlanMilestoneChecks
{
    public static void SeasonalFloorPlanMilestoneStartsInDraft()
    {
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("code-365", "label-47");
        Check.Equal(item.Status, SeasonalFloorPlanMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFloorPlanMilestoneMovesToSuspended()
    {
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("code-365", "label-47");
        try
        {
            item.MoveTo(SeasonalFloorPlanMilestoneStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFloorPlanMilestoneStatus.Suspended, "status");
    }

    public static void SeasonalFloorPlanMilestoneRejectsMovesFromClosed()
    {
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("code-365", "label-47");
        item.Status = SeasonalFloorPlanMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalFloorPlanMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanMilestoneStartsInDraft();
        SeasonalFloorPlanMilestoneMovesToSuspended();
        SeasonalFloorPlanMilestoneRejectsMovesFromClosed();
    }
}
