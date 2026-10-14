namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class RollingFeedbackScoreEngineChecks
{
    public static void RollingFeedbackScoreEngineKeepsResultsWithinBounds()
    {
        var engine = RollingFeedbackScoreEngine.DefaultRollingFeedbackScoreEngine();
        var item = RollingFeedbackScore.NewRollingFeedbackScore("code-825", "name-669");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingFeedbackScoreEngineKeepsResultsWithinBounds();
    }
}
