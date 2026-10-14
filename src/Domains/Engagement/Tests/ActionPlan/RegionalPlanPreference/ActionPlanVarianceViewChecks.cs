namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceViewChecks
{
    public static void ActionPlanVarianceViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
        var view = ActionPlanVarianceView.ToActionPlanVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanVarianceView.ToActionPlanVarianceViews(new List<ActionPlanVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanVarianceViewShowsTheStatusInCapitals();
    }
}
