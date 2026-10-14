namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanSegmentChecks
{
    public static void ReturnPlanSegmentStartsInIdle()
    {
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
        Check.Equal(item.Status, ReturnPlanSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanSegmentMovesToDraining()
    {
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
        try
        {
            item.MoveTo(ReturnPlanSegmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanSegmentStatus.Draining, "status");
    }

    public static void ReturnPlanSegmentRejectsMovesFromStopped()
    {
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
        item.Status = ReturnPlanSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReturnPlanSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReturnPlanSegmentStartsInIdle();
        ReturnPlanSegmentMovesToDraining();
        ReturnPlanSegmentRejectsMovesFromStopped();
    }
}
