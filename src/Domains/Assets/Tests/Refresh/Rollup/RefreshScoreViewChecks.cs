namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreViewChecks
{
    public static void RefreshScoreViewShowsTheStatusInCapitals()
    {
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
        var view = RefreshScoreView.ToRefreshScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshScoreView.ToRefreshScoreViews(new List<RefreshScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshScoreViewShowsTheStatusInCapitals();
    }
}
