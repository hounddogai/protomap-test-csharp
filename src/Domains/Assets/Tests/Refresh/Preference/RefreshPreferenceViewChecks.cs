namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshPreferenceViewChecks
{
    public static void RefreshPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RefreshPreference.NewRefreshPreference("code-364", "label-908");
        var view = RefreshPreferenceView.ToRefreshPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshPreferenceView.ToRefreshPreferenceViews(new List<RefreshPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshPreferenceViewShowsTheStatusInCapitals();
    }
}
