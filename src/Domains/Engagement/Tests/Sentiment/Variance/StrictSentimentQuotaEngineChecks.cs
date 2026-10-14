namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictSentimentQuotaEngineChecks
{
    public static void StrictSentimentQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictSentimentQuotaEngine.DefaultStrictSentimentQuotaEngine();
        var item = StrictSentimentQuota.NewStrictSentimentQuota("code-661", "label-631");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictSentimentQuotaEngineKeepsResultsWithinBounds();
    }
}
