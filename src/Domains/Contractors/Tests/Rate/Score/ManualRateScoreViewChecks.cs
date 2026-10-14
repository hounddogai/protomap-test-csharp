namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ManualRateScoreViewChecks
{
    public static void ManualRateScoreViewShowsTheStatusInCapitals()
    {
        var item = ManualRateScore.NewManualRateScore("code-861", "name-357");
        var view = ManualRateScoreView.ToManualRateScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualRateScoreView.ToManualRateScoreViews(new List<ManualRateScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualRateScoreViewShowsTheStatusInCapitals();
    }
}
