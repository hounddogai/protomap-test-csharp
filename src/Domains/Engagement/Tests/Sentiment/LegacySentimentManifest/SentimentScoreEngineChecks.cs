namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SentimentScoreEngineChecks
{
    public static void SentimentScoreEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentScoreEngine.DefaultSentimentScoreEngine(Flags.NewRegistry());
        var item = SentimentScore.NewSentimentScore("code-740", "name-118");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentScoreEngineKeepsResultsWithinBounds();
    }
}
