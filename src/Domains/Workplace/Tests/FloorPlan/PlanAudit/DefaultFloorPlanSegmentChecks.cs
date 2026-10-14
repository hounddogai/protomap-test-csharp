namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultFloorPlanSegmentChecks
{
    public static void DefaultFloorPlanSegmentStartsInNew()
    {
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
        Check.Equal(item.Status, DefaultFloorPlanSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultFloorPlanSegmentMovesToRunning()
    {
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
        try
        {
            item.MoveTo(DefaultFloorPlanSegmentStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultFloorPlanSegmentStatus.Running, "status");
    }

    public static void DefaultFloorPlanSegmentRejectsMovesFromFailed()
    {
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
        item.Status = DefaultFloorPlanSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultFloorPlanSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultFloorPlanSegmentStartsInNew();
        DefaultFloorPlanSegmentMovesToRunning();
        DefaultFloorPlanSegmentRejectsMovesFromFailed();
    }
}
