namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanItemChecks
{
    public static void ActionPlanItemStartsInRequested()
    {
        var item = ActionPlanItem.NewActionPlanItem("code-818", "name-425");
        Check.Equal(item.Status, ActionPlanItemStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanItemMovesToRevoked()
    {
        var item = ActionPlanItem.NewActionPlanItem("code-818", "name-425");
        try
        {
            item.MoveTo(ActionPlanItemStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanItemStatus.Revoked, "status");
    }

    public static void ActionPlanItemRejectsMovesFromExpired()
    {
        var item = ActionPlanItem.NewActionPlanItem("code-818", "name-425");
        item.Status = ActionPlanItemStatus.Expired;
        Check.Fails(() => item.MoveTo(ActionPlanItemStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ActionPlanItemStartsInRequested();
        ActionPlanItemMovesToRevoked();
        ActionPlanItemRejectsMovesFromExpired();
    }
}
