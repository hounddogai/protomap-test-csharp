namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementPreferenceViewChecks
{
    public static void EngagementPreferenceViewShowsTheStatusInCapitals()
    {
        var item = EngagementPreference.NewEngagementPreference("code-908", "name-996");
        var view = EngagementPreferenceView.ToEngagementPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementPreferenceView.ToEngagementPreferenceViews(new List<EngagementPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementPreferenceViewShowsTheStatusInCapitals();
    }
}
