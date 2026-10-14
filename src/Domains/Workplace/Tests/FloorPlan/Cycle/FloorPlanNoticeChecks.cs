namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanNoticeChecks
{
    public static void FloorPlanNoticeStartsInProposed()
    {
        var item = FloorPlanNotice.NewFloorPlanNotice("code-200", "title-409");
        Check.Equal(item.Status, FloorPlanNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanNoticeMovesToApproved()
    {
        var item = FloorPlanNotice.NewFloorPlanNotice("code-200", "title-409");
        try
        {
            item.MoveTo(FloorPlanNoticeStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanNoticeStatus.Approved, "status");
    }

    public static void FloorPlanNoticeRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanNotice.NewFloorPlanNotice("code-200", "title-409");
        item.Status = FloorPlanNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanNoticeStartsInProposed();
        FloorPlanNoticeMovesToApproved();
        FloorPlanNoticeRejectsMovesFromWithdrawn();
    }
}
