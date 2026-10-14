namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentBatchEngineChecks
{
    public static void SentimentBatchEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentBatchEngine.DefaultSentimentBatchEngine(Flags.NewRegistry());
        var item = SentimentBatch.NewSentimentBatch("code-32", "title-89");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentBatchEngineKeepsResultsWithinBounds();
    }
}
