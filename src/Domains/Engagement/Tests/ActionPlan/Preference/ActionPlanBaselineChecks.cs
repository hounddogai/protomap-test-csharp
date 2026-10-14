namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanBaselineChecks
{
    public static void ActionPlanBaselineStartsInProposed()
    {
        var item = ActionPlanBaseline.NewActionPlanBaseline("code-312", "label-658");
        Check.Equal(item.Status, ActionPlanBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanBaselineMovesToRejected()
    {
        var item = ActionPlanBaseline.NewActionPlanBaseline("code-312", "label-658");
        try
        {
            item.MoveTo(ActionPlanBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanBaselineStatus.Rejected, "status");
    }

    public static void ActionPlanBaselineRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanBaseline.NewActionPlanBaseline("code-312", "label-658");
        item.Status = ActionPlanBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanBaselineStartsInProposed();
        ActionPlanBaselineMovesToRejected();
        ActionPlanBaselineRejectsMovesFromWithdrawn();
    }
}
