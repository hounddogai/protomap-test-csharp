namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDigestChecks
{
    public static void ActionPlanDigestStartsInOpen()
    {
        var item = ActionPlanDigest.NewActionPlanDigest("code-803", "title-603");
        Check.Equal(item.Status, ActionPlanDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanDigestMovesToResolved()
    {
        var item = ActionPlanDigest.NewActionPlanDigest("code-803", "title-603");
        try
        {
            item.MoveTo(ActionPlanDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanDigestStatus.Resolved, "status");
    }

    public static void ActionPlanDigestRejectsMovesFromArchived()
    {
        var item = ActionPlanDigest.NewActionPlanDigest("code-803", "title-603");
        item.Status = ActionPlanDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(ActionPlanDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        ActionPlanDigestStartsInOpen();
        ActionPlanDigestMovesToResolved();
        ActionPlanDigestRejectsMovesFromArchived();
    }
}
