namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalSentimentSnapshotEngineChecks
{
    public static void RegionalSentimentSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSentimentSnapshotEngine.DefaultRegionalSentimentSnapshotEngine();
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("code-411", "label-926");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSentimentSnapshotEngineKeepsResultsWithinBounds();
    }
}
