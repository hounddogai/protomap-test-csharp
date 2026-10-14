namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalFloorPlanCheckpointChecks
{
    public static void SeasonalFloorPlanCheckpointStartsInIdle()
    {
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("code-597", "name-943");
        Check.Equal(item.Status, SeasonalFloorPlanCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFloorPlanCheckpointMovesToDraining()
    {
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("code-597", "name-943");
        try
        {
            item.MoveTo(SeasonalFloorPlanCheckpointStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFloorPlanCheckpointStatus.Draining, "status");
    }

    public static void SeasonalFloorPlanCheckpointRejectsMovesFromStopped()
    {
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("code-597", "name-943");
        item.Status = SeasonalFloorPlanCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalFloorPlanCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanCheckpointStartsInIdle();
        SeasonalFloorPlanCheckpointMovesToDraining();
        SeasonalFloorPlanCheckpointRejectsMovesFromStopped();
    }
}
