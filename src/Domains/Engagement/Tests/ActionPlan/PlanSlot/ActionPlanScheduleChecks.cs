namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanScheduleChecks
{
    public static void ActionPlanScheduleStartsInDraft()
    {
        var item = ActionPlanSchedule.NewActionPlanSchedule("code-946", "name-141");
        Check.Equal(item.Status, ActionPlanScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanScheduleMovesToActive()
    {
        var item = ActionPlanSchedule.NewActionPlanSchedule("code-946", "name-141");
        try
        {
            item.MoveTo(ActionPlanScheduleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanScheduleStatus.Active, "status");
    }

    public static void ActionPlanScheduleRejectsMovesFromClosed()
    {
        var item = ActionPlanSchedule.NewActionPlanSchedule("code-946", "name-141");
        item.Status = ActionPlanScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(ActionPlanScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ActionPlanScheduleStartsInDraft();
        ActionPlanScheduleMovesToActive();
        ActionPlanScheduleRejectsMovesFromClosed();
    }
}
