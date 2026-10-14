namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestEngineChecks
{
    public static void SentimentDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentDigestEngine.DefaultSentimentDigestEngine();
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentDigestEngineKeepsResultsWithinBounds();
    }
}
