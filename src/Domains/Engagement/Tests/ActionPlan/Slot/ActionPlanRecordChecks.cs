namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanRecordChecks
{
    public static void ActionPlanRecordStartsInPlanned()
    {
        var item = ActionPlanRecord.NewActionPlanRecord("code-41", "name-279");
        Check.Equal(item.Status, ActionPlanRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanRecordMovesToInProgress()
    {
        var item = ActionPlanRecord.NewActionPlanRecord("code-41", "name-279");
        try
        {
            item.MoveTo(ActionPlanRecordStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanRecordStatus.InProgress, "status");
    }

    public static void ActionPlanRecordRejectsMovesFromCancelled()
    {
        var item = ActionPlanRecord.NewActionPlanRecord("code-41", "name-279");
        item.Status = ActionPlanRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanRecordStartsInPlanned();
        ActionPlanRecordMovesToInProgress();
        ActionPlanRecordRejectsMovesFromCancelled();
    }
}
