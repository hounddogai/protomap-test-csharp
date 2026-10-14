namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingSentimentBatchEngineChecks
{
    public static void PendingSentimentBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PendingSentimentBatchEngine.DefaultPendingSentimentBatchEngine();
        var item = PendingSentimentBatch.NewPendingSentimentBatch("code-343", "name-803");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingSentimentBatchEngineKeepsResultsWithinBounds();
    }
}
