namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanThresholdChecks
{
    public static void ActionPlanThresholdStartsInIdle()
    {
        var item = ActionPlanThreshold.NewActionPlanThreshold("code-499", "label-912");
        Check.Equal(item.Status, ActionPlanThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanThresholdMovesToReady()
    {
        var item = ActionPlanThreshold.NewActionPlanThreshold("code-499", "label-912");
        try
        {
            item.MoveTo(ActionPlanThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanThresholdStatus.Ready, "status");
    }

    public static void ActionPlanThresholdRejectsMovesFromStopped()
    {
        var item = ActionPlanThreshold.NewActionPlanThreshold("code-499", "label-912");
        item.Status = ActionPlanThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(ActionPlanThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ActionPlanThresholdStartsInIdle();
        ActionPlanThresholdMovesToReady();
        ActionPlanThresholdRejectsMovesFromStopped();
    }
}
