namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentSnapshotEngineChecks
{
    public static void SentimentSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentSnapshotEngine.DefaultSentimentSnapshotEngine(Flags.NewRegistry());
        var item = SentimentSnapshot.NewSentimentSnapshot("code-274", "title-498");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentSnapshotEngineKeepsResultsWithinBounds();
    }
}
