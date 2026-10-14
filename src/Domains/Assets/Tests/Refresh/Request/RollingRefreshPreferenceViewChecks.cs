namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RollingRefreshPreferenceViewChecks
{
    public static void RollingRefreshPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RollingRefreshPreference.NewRollingRefreshPreference("code-129", "label-427");
        var view = RollingRefreshPreferenceView.ToRollingRefreshPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRefreshPreferenceView.ToRollingRefreshPreferenceViews(new List<RollingRefreshPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRefreshPreferenceViewShowsTheStatusInCapitals();
    }
}
