namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentQuotaEngineChecks
{
    public static void SentimentQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentQuotaEngine.DefaultSentimentQuotaEngine();
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentQuotaEngineKeepsResultsWithinBounds();
    }
}
