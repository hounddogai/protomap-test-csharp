namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ActionPlanMetricViewChecks
{
    public static void ActionPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanMetric.NewActionPlanMetric("code-534", "title-933");
        var view = ActionPlanMetricView.ToActionPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanMetricView.ToActionPlanMetricViews(new List<ActionPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanMetricViewShowsTheStatusInCapitals();
    }
}
