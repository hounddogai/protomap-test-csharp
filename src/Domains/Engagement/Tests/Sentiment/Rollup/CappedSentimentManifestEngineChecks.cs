namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestEngineChecks
{
    public static void CappedSentimentManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSentimentManifestEngine.DefaultCappedSentimentManifestEngine(Flags.NewRegistry());
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSentimentManifestEngineKeepsResultsWithinBounds();
    }
}
