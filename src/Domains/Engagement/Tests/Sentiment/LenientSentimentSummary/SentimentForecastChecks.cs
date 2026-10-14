namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentForecastChecks
{
    public static void SentimentForecastStartsInDraft()
    {
        var item = SentimentForecast.NewSentimentForecast("code-533", "title-299");
        Check.Equal(item.Status, SentimentForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentForecastMovesToPending()
    {
        var item = SentimentForecast.NewSentimentForecast("code-533", "title-299");
        try
        {
            item.MoveTo(SentimentForecastStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentForecastStatus.Pending, "status");
    }

    public static void SentimentForecastRejectsMovesFromClosed()
    {
        var item = SentimentForecast.NewSentimentForecast("code-533", "title-299");
        item.Status = SentimentForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(SentimentForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SentimentForecastStartsInDraft();
        SentimentForecastMovesToPending();
        SentimentForecastRejectsMovesFromClosed();
    }
}
