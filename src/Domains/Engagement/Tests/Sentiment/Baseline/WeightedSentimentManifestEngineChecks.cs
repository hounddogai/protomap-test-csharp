namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedSentimentManifestEngineChecks
{
    public static void WeightedSentimentManifestEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedSentimentManifestEngine.DefaultWeightedSentimentManifestEngine();
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedSentimentManifestEngineKeepsResultsWithinBounds();
    }
}
