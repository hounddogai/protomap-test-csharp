namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentSummaryEngineChecks
{
    public static void SentimentSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentSummaryEngine.DefaultSentimentSummaryEngine(Flags.NewRegistry());
        var item = SentimentSummary.NewSentimentSummary("code-835", "title-698");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentSummaryEngineKeepsResultsWithinBounds();
    }
}
