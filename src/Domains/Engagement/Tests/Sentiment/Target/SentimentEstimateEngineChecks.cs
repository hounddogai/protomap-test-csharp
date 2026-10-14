namespace Acme.Workforce.Engagement.Tests.Sentiment.Target;

using Acme.Workforce.Engagement.Sentiment.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SentimentEstimateEngineChecks
{
    public static void SentimentEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentEstimateEngine.DefaultSentimentEstimateEngine(Flags.NewRegistry());
        var item = SentimentEstimate.NewSentimentEstimate("code-885", "label-545");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentEstimateEngineKeepsResultsWithinBounds();
    }
}
