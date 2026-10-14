namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleChecks
{
    public static void ActionPlanBundleStartsInNew()
    {
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
        Check.Equal(item.Status, ActionPlanBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanBundleMovesToRunning()
    {
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
        try
        {
            item.MoveTo(ActionPlanBundleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanBundleStatus.Running, "status");
    }

    public static void ActionPlanBundleRejectsMovesFromFailed()
    {
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
        item.Status = ActionPlanBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(ActionPlanBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        ActionPlanBundleStartsInNew();
        ActionPlanBundleMovesToRunning();
        ActionPlanBundleRejectsMovesFromFailed();
    }
}
