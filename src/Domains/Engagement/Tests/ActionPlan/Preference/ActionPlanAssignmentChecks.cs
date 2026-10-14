namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanAssignmentChecks
{
    public static void ActionPlanAssignmentStartsInPlanned()
    {
        var item = ActionPlanAssignment.NewActionPlanAssignment("code-73", "title-697");
        Check.Equal(item.Status, ActionPlanAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanAssignmentMovesToScheduled()
    {
        var item = ActionPlanAssignment.NewActionPlanAssignment("code-73", "title-697");
        try
        {
            item.MoveTo(ActionPlanAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanAssignmentStatus.Scheduled, "status");
    }

    public static void ActionPlanAssignmentRejectsMovesFromCancelled()
    {
        var item = ActionPlanAssignment.NewActionPlanAssignment("code-73", "title-697");
        item.Status = ActionPlanAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanAssignmentStartsInPlanned();
        ActionPlanAssignmentMovesToScheduled();
        ActionPlanAssignmentRejectsMovesFromCancelled();
    }
}
