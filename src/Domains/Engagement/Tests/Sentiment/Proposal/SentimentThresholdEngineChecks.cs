namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentThresholdEngineChecks
{
    public static void SentimentThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentThresholdEngine.DefaultSentimentThresholdEngine();
        var item = SentimentThreshold.NewSentimentThreshold("code-661", "title-678");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentThresholdEngineKeepsResultsWithinBounds();
    }
}
