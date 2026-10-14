namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class DefaultEngagementPreferenceViewChecks
{
    public static void DefaultEngagementPreferenceViewShowsTheStatusInCapitals()
    {
        var item = DefaultEngagementPreference.NewDefaultEngagementPreference("code-882", "name-335");
        var view = DefaultEngagementPreferenceView.ToDefaultEngagementPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultEngagementPreferenceView.ToDefaultEngagementPreferenceViews(new List<DefaultEngagementPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultEngagementPreferenceViewShowsTheStatusInCapitals();
    }
}
