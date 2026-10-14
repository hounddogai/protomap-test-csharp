namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentMetricEngineChecks
{
    public static void SentimentMetricEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentMetricEngine.DefaultSentimentMetricEngine(Flags.NewRegistry());
        var item = SentimentMetric.NewSentimentMetric("code-774", "name-977");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentMetricEngineKeepsResultsWithinBounds();
    }
}
