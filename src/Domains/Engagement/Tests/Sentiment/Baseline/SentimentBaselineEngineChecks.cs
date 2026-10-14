namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SentimentBaselineEngineChecks
{
    public static void SentimentBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentBaselineEngine.DefaultSentimentBaselineEngine(Flags.NewRegistry());
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentBaselineEngineKeepsResultsWithinBounds();
    }
}
