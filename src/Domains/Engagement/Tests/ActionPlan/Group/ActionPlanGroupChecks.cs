namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ActionPlanGroupChecks
{
    public static void ActionPlanGroupStartsInPlanned()
    {
        var item = ActionPlanGroup.NewActionPlanGroup("code-415", "label-523");
        Check.Equal(item.Status, ActionPlanGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanGroupMovesToInProgress()
    {
        var item = ActionPlanGroup.NewActionPlanGroup("code-415", "label-523");
        try
        {
            item.MoveTo(ActionPlanGroupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanGroupStatus.InProgress, "status");
    }

    public static void ActionPlanGroupRejectsMovesFromCancelled()
    {
        var item = ActionPlanGroup.NewActionPlanGroup("code-415", "label-523");
        item.Status = ActionPlanGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanGroupStartsInPlanned();
        ActionPlanGroupMovesToInProgress();
        ActionPlanGroupRejectsMovesFromCancelled();
    }
}
