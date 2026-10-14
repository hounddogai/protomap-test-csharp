namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanWindowChecks
{
    public static void ActionPlanWindowStartsInOpen()
    {
        var item = ActionPlanWindow.NewActionPlanWindow("code-825", "title-370");
        Check.Equal(item.Status, ActionPlanWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanWindowMovesToResolved()
    {
        var item = ActionPlanWindow.NewActionPlanWindow("code-825", "title-370");
        try
        {
            item.MoveTo(ActionPlanWindowStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanWindowStatus.Resolved, "status");
    }

    public static void ActionPlanWindowRejectsMovesFromArchived()
    {
        var item = ActionPlanWindow.NewActionPlanWindow("code-825", "title-370");
        item.Status = ActionPlanWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(ActionPlanWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        ActionPlanWindowStartsInOpen();
        ActionPlanWindowMovesToResolved();
        ActionPlanWindowRejectsMovesFromArchived();
    }
}
