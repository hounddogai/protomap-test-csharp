namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestChecks
{
    public static void ActionPlanRequestStartsInDraft()
    {
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
        Check.Equal(item.Status, ActionPlanRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanRequestMovesToActive()
    {
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
        try
        {
            item.MoveTo(ActionPlanRequestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanRequestStatus.Active, "status");
    }

    public static void ActionPlanRequestRejectsMovesFromClosed()
    {
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
        item.Status = ActionPlanRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(ActionPlanRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ActionPlanRequestStartsInDraft();
        ActionPlanRequestMovesToActive();
        ActionPlanRequestRejectsMovesFromClosed();
    }
}
