namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanSegmentChecks
{
    public static void ActionPlanSegmentStartsInPlanned()
    {
        var item = ActionPlanSegment.NewActionPlanSegment("code-533", "title-767");
        Check.Equal(item.Status, ActionPlanSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanSegmentMovesToCancelled()
    {
        var item = ActionPlanSegment.NewActionPlanSegment("code-533", "title-767");
        try
        {
            item.MoveTo(ActionPlanSegmentStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanSegmentStatus.Cancelled, "status");
    }

    public static void ActionPlanSegmentRejectsMovesFromCancelled()
    {
        var item = ActionPlanSegment.NewActionPlanSegment("code-533", "title-767");
        item.Status = ActionPlanSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanSegmentStartsInPlanned();
        ActionPlanSegmentMovesToCancelled();
        ActionPlanSegmentRejectsMovesFromCancelled();
    }
}
