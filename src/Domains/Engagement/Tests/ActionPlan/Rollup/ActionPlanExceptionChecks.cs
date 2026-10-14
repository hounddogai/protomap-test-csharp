namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionChecks
{
    public static void ActionPlanExceptionStartsInProposed()
    {
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
        Check.Equal(item.Status, ActionPlanExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanExceptionMovesToRejected()
    {
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
        try
        {
            item.MoveTo(ActionPlanExceptionStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanExceptionStatus.Rejected, "status");
    }

    public static void ActionPlanExceptionRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
        item.Status = ActionPlanExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanExceptionStartsInProposed();
        ActionPlanExceptionMovesToRejected();
        ActionPlanExceptionRejectsMovesFromWithdrawn();
    }
}
