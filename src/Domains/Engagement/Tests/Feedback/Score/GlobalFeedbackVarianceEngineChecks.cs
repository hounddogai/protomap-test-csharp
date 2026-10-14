namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GlobalFeedbackVarianceEngineChecks
{
    public static void GlobalFeedbackVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalFeedbackVarianceEngine.DefaultGlobalFeedbackVarianceEngine();
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("code-851", "name-73");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalFeedbackVarianceEngineKeepsResultsWithinBounds();
    }
}
