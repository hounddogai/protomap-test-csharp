namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class FallbackActionPlanSegmentChecks
{
    public static void FallbackActionPlanSegmentStartsInRequested()
    {
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
        Check.Equal(item.Status, FallbackActionPlanSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackActionPlanSegmentMovesToRevoked()
    {
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
        try
        {
            item.MoveTo(FallbackActionPlanSegmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackActionPlanSegmentStatus.Revoked, "status");
    }

    public static void FallbackActionPlanSegmentRejectsMovesFromExpired()
    {
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
        item.Status = FallbackActionPlanSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackActionPlanSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackActionPlanSegmentStartsInRequested();
        FallbackActionPlanSegmentMovesToRevoked();
        FallbackActionPlanSegmentRejectsMovesFromExpired();
    }
}
