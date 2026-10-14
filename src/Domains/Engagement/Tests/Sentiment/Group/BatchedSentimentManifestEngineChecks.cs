namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedSentimentManifestEngineChecks
{
    public static void BatchedSentimentManifestEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedSentimentManifestEngine.DefaultBatchedSentimentManifestEngine(Flags.NewRegistry());
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("code-314", "name-707");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedSentimentManifestEngineKeepsResultsWithinBounds();
    }
}
