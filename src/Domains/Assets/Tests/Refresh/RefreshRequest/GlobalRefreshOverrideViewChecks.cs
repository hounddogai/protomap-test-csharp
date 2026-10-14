namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class GlobalRefreshOverrideViewChecks
{
    public static void GlobalRefreshOverrideViewShowsTheStatusInCapitals()
    {
        var item = GlobalRefreshOverride.NewGlobalRefreshOverride("code-486", "name-402");
        var view = GlobalRefreshOverrideView.ToGlobalRefreshOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalRefreshOverrideView.ToGlobalRefreshOverrideViews(new List<GlobalRefreshOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalRefreshOverrideViewShowsTheStatusInCapitals();
    }
}
