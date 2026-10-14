namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanBaselineViewChecks
{
    public static void ActionPlanBaselineViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanBaseline.NewActionPlanBaseline("code-312", "label-658");
        var view = ActionPlanBaselineView.ToActionPlanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanBaselineView.ToActionPlanBaselineViews(new List<ActionPlanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanBaselineViewShowsTheStatusInCapitals();
    }
}
