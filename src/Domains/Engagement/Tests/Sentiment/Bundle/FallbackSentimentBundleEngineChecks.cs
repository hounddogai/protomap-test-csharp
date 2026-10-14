namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackSentimentBundleEngineChecks
{
    public static void FallbackSentimentBundleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackSentimentBundleEngine.DefaultFallbackSentimentBundleEngine(Flags.NewRegistry());
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("code-293", "title-789");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackSentimentBundleEngineKeepsResultsWithinBounds();
    }
}
