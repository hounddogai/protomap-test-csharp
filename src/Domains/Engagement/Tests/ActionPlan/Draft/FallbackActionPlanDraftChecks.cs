namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackActionPlanDraftChecks
{
    public static void FallbackActionPlanDraftStartsInRequested()
    {
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("code-538", "label-842");
        Check.Equal(item.Status, FallbackActionPlanDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackActionPlanDraftMovesToExpired()
    {
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("code-538", "label-842");
        try
        {
            item.MoveTo(FallbackActionPlanDraftStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackActionPlanDraftStatus.Expired, "status");
    }

    public static void FallbackActionPlanDraftRejectsMovesFromExpired()
    {
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("code-538", "label-842");
        item.Status = FallbackActionPlanDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackActionPlanDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackActionPlanDraftStartsInRequested();
        FallbackActionPlanDraftMovesToExpired();
        FallbackActionPlanDraftRejectsMovesFromExpired();
    }
}
