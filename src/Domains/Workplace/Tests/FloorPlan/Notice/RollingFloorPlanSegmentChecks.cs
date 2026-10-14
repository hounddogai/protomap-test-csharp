namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingFloorPlanSegmentChecks
{
    public static void RollingFloorPlanSegmentStartsInIdle()
    {
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("code-61", "name-220");
        Check.Equal(item.Status, RollingFloorPlanSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingFloorPlanSegmentMovesToWarming()
    {
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("code-61", "name-220");
        try
        {
            item.MoveTo(RollingFloorPlanSegmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingFloorPlanSegmentStatus.Warming, "status");
    }

    public static void RollingFloorPlanSegmentRejectsMovesFromStopped()
    {
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("code-61", "name-220");
        item.Status = RollingFloorPlanSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingFloorPlanSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingFloorPlanSegmentStartsInIdle();
        RollingFloorPlanSegmentMovesToWarming();
        RollingFloorPlanSegmentRejectsMovesFromStopped();
    }
}
