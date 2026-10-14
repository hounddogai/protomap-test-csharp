namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentPreferenceEngineChecks
{
    public static void FallbackSentimentPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackSentimentPreferenceEngine.DefaultFallbackSentimentPreferenceEngine();
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("code-704", "title-412");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackSentimentPreferenceEngineKeepsResultsWithinBounds();
    }
}
