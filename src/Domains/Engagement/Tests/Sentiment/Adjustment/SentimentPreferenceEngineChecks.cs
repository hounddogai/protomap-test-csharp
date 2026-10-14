namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentPreferenceEngineChecks
{
    public static void SentimentPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentPreferenceEngine.DefaultSentimentPreferenceEngine();
        var item = SentimentPreference.NewSentimentPreference("code-911", "title-334");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentPreferenceEngineKeepsResultsWithinBounds();
    }
}
