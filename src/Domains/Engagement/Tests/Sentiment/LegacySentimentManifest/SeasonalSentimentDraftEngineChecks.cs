namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftEngineChecks
{
    public static void SeasonalSentimentDraftEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSentimentDraftEngine.DefaultSeasonalSentimentDraftEngine(Flags.NewRegistry());
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSentimentDraftEngineKeepsResultsWithinBounds();
    }
}
