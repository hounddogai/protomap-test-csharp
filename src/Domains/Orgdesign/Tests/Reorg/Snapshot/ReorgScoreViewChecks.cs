namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgScoreViewChecks
{
    public static void ReorgScoreViewShowsTheStatusInCapitals()
    {
        var item = ReorgScore.NewReorgScore("code-749", "label-261");
        var view = ReorgScoreView.ToReorgScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgScoreView.ToReorgScoreViews(new List<ReorgScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgScoreViewShowsTheStatusInCapitals();
    }
}
