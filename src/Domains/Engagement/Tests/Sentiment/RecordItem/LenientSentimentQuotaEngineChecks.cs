namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientSentimentQuotaEngineChecks
{
    public static void LenientSentimentQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSentimentQuotaEngine.DefaultLenientSentimentQuotaEngine(Flags.NewRegistry());
        var item = LenientSentimentQuota.NewLenientSentimentQuota("code-66", "title-774");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSentimentQuotaEngineKeepsResultsWithinBounds();
    }
}
