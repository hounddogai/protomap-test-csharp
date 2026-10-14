namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordChecks
{
    public static void SeasonalFloorPlanRecordStartsInOpen()
    {
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
        Check.Equal(item.Status, SeasonalFloorPlanRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFloorPlanRecordMovesToResolved()
    {
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
        try
        {
            item.MoveTo(SeasonalFloorPlanRecordStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFloorPlanRecordStatus.Resolved, "status");
    }

    public static void SeasonalFloorPlanRecordRejectsMovesFromArchived()
    {
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
        item.Status = SeasonalFloorPlanRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalFloorPlanRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanRecordStartsInOpen();
        SeasonalFloorPlanRecordMovesToResolved();
        SeasonalFloorPlanRecordRejectsMovesFromArchived();
    }
}
