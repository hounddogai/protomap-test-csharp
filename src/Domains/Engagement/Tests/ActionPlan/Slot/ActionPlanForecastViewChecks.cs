namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanForecastViewChecks
{
    public static void ActionPlanForecastViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanForecast.NewActionPlanForecast("code-260", "title-64");
        var view = ActionPlanForecastView.ToActionPlanForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanForecastView.ToActionPlanForecastViews(new List<ActionPlanForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanForecastViewShowsTheStatusInCapitals();
    }
}
