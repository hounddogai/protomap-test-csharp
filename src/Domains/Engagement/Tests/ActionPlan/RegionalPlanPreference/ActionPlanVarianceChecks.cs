namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceChecks
{
    public static void ActionPlanVarianceStartsInNew()
    {
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
        Check.Equal(item.Status, ActionPlanVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanVarianceMovesToRunning()
    {
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
        try
        {
            item.MoveTo(ActionPlanVarianceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanVarianceStatus.Running, "status");
    }

    public static void ActionPlanVarianceRejectsMovesFromFailed()
    {
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
        item.Status = ActionPlanVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(ActionPlanVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        ActionPlanVarianceStartsInNew();
        ActionPlanVarianceMovesToRunning();
        ActionPlanVarianceRejectsMovesFromFailed();
    }
}
