namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentClaimEngineChecks
{
    public static void SentimentClaimEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentClaimEngine.DefaultSentimentClaimEngine(Flags.NewRegistry());
        var item = SentimentClaim.NewSentimentClaim("code-205", "title-722");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentClaimEngineKeepsResultsWithinBounds();
    }
}
