namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedRefreshScoreViewChecks
{
    public static void WeightedRefreshScoreViewShowsTheStatusInCapitals()
    {
        var item = WeightedRefreshScore.NewWeightedRefreshScore("code-195", "label-272");
        var view = WeightedRefreshScoreView.ToWeightedRefreshScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedRefreshScoreView.ToWeightedRefreshScoreViews(new List<WeightedRefreshScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedRefreshScoreViewShowsTheStatusInCapitals();
    }
}
