namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanForecastChecks
{
    public static void ActionPlanForecastStartsInDraft()
    {
        var item = ActionPlanForecast.NewActionPlanForecast("code-260", "title-64");
        Check.Equal(item.Status, ActionPlanForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanForecastMovesToActive()
    {
        var item = ActionPlanForecast.NewActionPlanForecast("code-260", "title-64");
        try
        {
            item.MoveTo(ActionPlanForecastStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanForecastStatus.Active, "status");
    }

    public static void ActionPlanForecastRejectsMovesFromClosed()
    {
        var item = ActionPlanForecast.NewActionPlanForecast("code-260", "title-64");
        item.Status = ActionPlanForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(ActionPlanForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ActionPlanForecastStartsInDraft();
        ActionPlanForecastMovesToActive();
        ActionPlanForecastRejectsMovesFromClosed();
    }
}
