namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentChecks
{
    public static void ActionPlanAdjustmentStartsInProposed()
    {
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
        Check.Equal(item.Status, ActionPlanAdjustmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanAdjustmentMovesToApproved()
    {
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
        try
        {
            item.MoveTo(ActionPlanAdjustmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanAdjustmentStatus.Approved, "status");
    }

    public static void ActionPlanAdjustmentRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
        item.Status = ActionPlanAdjustmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanAdjustmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanAdjustmentStartsInProposed();
        ActionPlanAdjustmentMovesToApproved();
        ActionPlanAdjustmentRejectsMovesFromWithdrawn();
    }
}
