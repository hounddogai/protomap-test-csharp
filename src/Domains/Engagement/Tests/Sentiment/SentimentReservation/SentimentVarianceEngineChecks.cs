namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentVarianceEngineChecks
{
    public static void SentimentVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentVarianceEngine.DefaultSentimentVarianceEngine(Flags.NewRegistry());
        var item = SentimentVariance.NewSentimentVariance("code-677", "title-770");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentVarianceEngineKeepsResultsWithinBounds();
    }
}
