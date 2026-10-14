namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class TieredSentimentManifestEngineChecks
{
    public static void TieredSentimentManifestEngineKeepsResultsWithinBounds()
    {
        var engine = TieredSentimentManifestEngine.DefaultTieredSentimentManifestEngine();
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredSentimentManifestEngineKeepsResultsWithinBounds();
    }
}
