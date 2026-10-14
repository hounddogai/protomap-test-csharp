namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSentimentAdjustmentEngineChecks
{
    public static void TieredSentimentAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = TieredSentimentAdjustmentEngine.DefaultTieredSentimentAdjustmentEngine();
        var item = TieredSentimentAdjustment.NewTieredSentimentAdjustment("code-846", "label-685");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredSentimentAdjustmentEngineKeepsResultsWithinBounds();
    }
}
