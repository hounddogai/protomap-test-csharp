namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class ManualSentimentVarianceEngineChecks
{
    public static void ManualSentimentVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSentimentVarianceEngine.DefaultManualSentimentVarianceEngine();
        var item = ManualSentimentVariance.NewManualSentimentVariance("code-757", "label-858");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSentimentVarianceEngineKeepsResultsWithinBounds();
    }
}
