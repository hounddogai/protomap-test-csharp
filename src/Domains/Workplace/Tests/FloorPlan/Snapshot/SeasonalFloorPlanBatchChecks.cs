namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalFloorPlanBatchChecks
{
    public static void SeasonalFloorPlanBatchStartsInRequested()
    {
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("code-23", "name-335");
        Check.Equal(item.Status, SeasonalFloorPlanBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFloorPlanBatchMovesToGranted()
    {
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("code-23", "name-335");
        try
        {
            item.MoveTo(SeasonalFloorPlanBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFloorPlanBatchStatus.Granted, "status");
    }

    public static void SeasonalFloorPlanBatchRejectsMovesFromExpired()
    {
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("code-23", "name-335");
        item.Status = SeasonalFloorPlanBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalFloorPlanBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanBatchStartsInRequested();
        SeasonalFloorPlanBatchMovesToGranted();
        SeasonalFloorPlanBatchRejectsMovesFromExpired();
    }
}
