namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredRecognitionScoreViewChecks
{
    public static void TieredRecognitionScoreViewShowsTheStatusInCapitals()
    {
        var item = TieredRecognitionScore.NewTieredRecognitionScore("code-61", "name-198");
        var view = TieredRecognitionScoreView.ToTieredRecognitionScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredRecognitionScoreView.ToTieredRecognitionScoreViews(new List<TieredRecognitionScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredRecognitionScoreViewShowsTheStatusInCapitals();
    }
}
