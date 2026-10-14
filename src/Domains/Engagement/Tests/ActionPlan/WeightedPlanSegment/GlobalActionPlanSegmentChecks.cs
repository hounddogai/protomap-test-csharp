namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanSegmentChecks
{
    public static void GlobalActionPlanSegmentStartsInRequested()
    {
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
        Check.Equal(item.Status, GlobalActionPlanSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalActionPlanSegmentMovesToExpired()
    {
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
        try
        {
            item.MoveTo(GlobalActionPlanSegmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalActionPlanSegmentStatus.Expired, "status");
    }

    public static void GlobalActionPlanSegmentRejectsMovesFromExpired()
    {
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
        item.Status = GlobalActionPlanSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalActionPlanSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalActionPlanSegmentStartsInRequested();
        GlobalActionPlanSegmentMovesToExpired();
        GlobalActionPlanSegmentRejectsMovesFromExpired();
    }
}
