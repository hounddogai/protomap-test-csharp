namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanMilestoneChecks
{
    public static void ActionPlanMilestoneStartsInProposed()
    {
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
        Check.Equal(item.Status, ActionPlanMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanMilestoneMovesToApproved()
    {
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
        try
        {
            item.MoveTo(ActionPlanMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanMilestoneStatus.Approved, "status");
    }

    public static void ActionPlanMilestoneRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
        item.Status = ActionPlanMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanMilestoneStartsInProposed();
        ActionPlanMilestoneMovesToApproved();
        ActionPlanMilestoneRejectsMovesFromWithdrawn();
    }
}
