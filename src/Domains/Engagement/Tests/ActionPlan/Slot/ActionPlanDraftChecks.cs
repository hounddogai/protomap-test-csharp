namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftChecks
{
    public static void ActionPlanDraftStartsInRequested()
    {
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
        Check.Equal(item.Status, ActionPlanDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanDraftMovesToRevoked()
    {
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
        try
        {
            item.MoveTo(ActionPlanDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanDraftStatus.Revoked, "status");
    }

    public static void ActionPlanDraftRejectsMovesFromExpired()
    {
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
        item.Status = ActionPlanDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(ActionPlanDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ActionPlanDraftStartsInRequested();
        ActionPlanDraftMovesToRevoked();
        ActionPlanDraftRejectsMovesFromExpired();
    }
}
