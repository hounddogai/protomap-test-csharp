namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class ActionPlanPolicyChecks
{
    public static void ActionPlanPolicyStartsInDraft()
    {
        var item = ActionPlanPolicy.NewActionPlanPolicy("code-271", "name-976");
        Check.Equal(item.Status, ActionPlanPolicyStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanPolicyMovesToPending()
    {
        var item = ActionPlanPolicy.NewActionPlanPolicy("code-271", "name-976");
        try
        {
            item.MoveTo(ActionPlanPolicyStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanPolicyStatus.Pending, "status");
    }

    public static void ActionPlanPolicyRejectsMovesFromClosed()
    {
        var item = ActionPlanPolicy.NewActionPlanPolicy("code-271", "name-976");
        item.Status = ActionPlanPolicyStatus.Closed;
        Check.Fails(() => item.MoveTo(ActionPlanPolicyStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ActionPlanPolicyStartsInDraft();
        ActionPlanPolicyMovesToPending();
        ActionPlanPolicyRejectsMovesFromClosed();
    }
}
