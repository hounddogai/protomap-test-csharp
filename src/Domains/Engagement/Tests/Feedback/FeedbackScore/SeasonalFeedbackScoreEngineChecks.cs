namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackScoreEngineChecks
{
    public static void SeasonalFeedbackScoreEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalFeedbackScoreEngine.DefaultSeasonalFeedbackScoreEngine();
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalFeedbackScoreEngineKeepsResultsWithinBounds();
    }
}
