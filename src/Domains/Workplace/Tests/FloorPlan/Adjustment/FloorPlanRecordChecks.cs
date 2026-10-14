namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordChecks
{
    public static void FloorPlanRecordStartsInOpen()
    {
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
        Check.Equal(item.Status, FloorPlanRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanRecordMovesToReopened()
    {
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
        try
        {
            item.MoveTo(FloorPlanRecordStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanRecordStatus.Reopened, "status");
    }

    public static void FloorPlanRecordRejectsMovesFromArchived()
    {
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
        item.Status = FloorPlanRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(FloorPlanRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        FloorPlanRecordStartsInOpen();
        FloorPlanRecordMovesToReopened();
        FloorPlanRecordRejectsMovesFromArchived();
    }
}
