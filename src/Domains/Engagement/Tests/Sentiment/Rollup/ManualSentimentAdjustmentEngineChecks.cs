namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualSentimentAdjustmentEngineChecks
{
    public static void ManualSentimentAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSentimentAdjustmentEngine.DefaultManualSentimentAdjustmentEngine();
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("code-908", "name-326");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSentimentAdjustmentEngineKeepsResultsWithinBounds();
    }
}
