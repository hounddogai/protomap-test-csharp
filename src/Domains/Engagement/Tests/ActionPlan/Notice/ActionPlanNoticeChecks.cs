namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ActionPlanNoticeChecks
{
    public static void ActionPlanNoticeStartsInProposed()
    {
        var item = ActionPlanNotice.NewActionPlanNotice("code-653", "name-323");
        Check.Equal(item.Status, ActionPlanNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanNoticeMovesToRejected()
    {
        var item = ActionPlanNotice.NewActionPlanNotice("code-653", "name-323");
        try
        {
            item.MoveTo(ActionPlanNoticeStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanNoticeStatus.Rejected, "status");
    }

    public static void ActionPlanNoticeRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanNotice.NewActionPlanNotice("code-653", "name-323");
        item.Status = ActionPlanNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanNoticeStartsInProposed();
        ActionPlanNoticeMovesToRejected();
        ActionPlanNoticeRejectsMovesFromWithdrawn();
    }
}
