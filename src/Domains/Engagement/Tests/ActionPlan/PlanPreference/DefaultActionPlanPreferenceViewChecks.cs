namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class DefaultActionPlanPreferenceViewChecks
{
    public static void DefaultActionPlanPreferenceViewShowsTheStatusInCapitals()
    {
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("code-206", "name-62");
        var view = DefaultActionPlanPreferenceView.ToDefaultActionPlanPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultActionPlanPreferenceView.ToDefaultActionPlanPreferenceViews(new List<DefaultActionPlanPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultActionPlanPreferenceViewShowsTheStatusInCapitals();
    }
}
