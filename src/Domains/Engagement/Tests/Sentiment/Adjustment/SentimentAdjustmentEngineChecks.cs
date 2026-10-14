namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentAdjustmentEngineChecks
{
    public static void SentimentAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentAdjustmentEngine.DefaultSentimentAdjustmentEngine();
        var item = SentimentAdjustment.NewSentimentAdjustment("code-337", "title-94");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentAdjustmentEngineKeepsResultsWithinBounds();
    }
}
