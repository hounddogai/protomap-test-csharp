namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanSummaryViewChecks
{
    public static void ActionPlanSummaryViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
        var view = ActionPlanSummaryView.ToActionPlanSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanSummaryView.ToActionPlanSummaryViews(new List<ActionPlanSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanSummaryViewShowsTheStatusInCapitals();
    }
}
