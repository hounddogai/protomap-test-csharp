namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentForecastEngineChecks
{
    public static void SentimentForecastEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentForecastEngine.DefaultSentimentForecastEngine();
        var item = SentimentForecast.NewSentimentForecast("code-533", "title-299");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentForecastEngineKeepsResultsWithinBounds();
    }
}
