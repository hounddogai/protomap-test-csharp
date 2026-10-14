namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanSegmentChecks
{
    public static void FloorPlanSegmentStartsInIdle()
    {
        var item = FloorPlanSegment.NewFloorPlanSegment("code-629", "title-649");
        Check.Equal(item.Status, FloorPlanSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanSegmentMovesToStopped()
    {
        var item = FloorPlanSegment.NewFloorPlanSegment("code-629", "title-649");
        try
        {
            item.MoveTo(FloorPlanSegmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanSegmentStatus.Stopped, "status");
    }

    public static void FloorPlanSegmentRejectsMovesFromStopped()
    {
        var item = FloorPlanSegment.NewFloorPlanSegment("code-629", "title-649");
        item.Status = FloorPlanSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FloorPlanSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FloorPlanSegmentStartsInIdle();
        FloorPlanSegmentMovesToStopped();
        FloorPlanSegmentRejectsMovesFromStopped();
    }
}
