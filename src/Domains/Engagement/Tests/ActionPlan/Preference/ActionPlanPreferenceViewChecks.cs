namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanPreferenceViewChecks
{
    public static void ActionPlanPreferenceViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
        var view = ActionPlanPreferenceView.ToActionPlanPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanPreferenceView.ToActionPlanPreferenceViews(new List<ActionPlanPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanPreferenceViewShowsTheStatusInCapitals();
    }
}
