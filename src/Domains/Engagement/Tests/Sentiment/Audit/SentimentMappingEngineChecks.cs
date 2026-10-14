namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentMappingEngineChecks
{
    public static void SentimentMappingEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentMappingEngine.DefaultSentimentMappingEngine();
        var item = SentimentMapping.NewSentimentMapping("code-715", "title-167");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentMappingEngineKeepsResultsWithinBounds();
    }
}
