namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackForecastChecks
{
    public static void FeedbackForecastStartsInDraft()
    {
        var item = FeedbackForecast.NewFeedbackForecast("code-987", "label-830");
        Check.Equal(item.Status, FeedbackForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackForecastMovesToClosed()
    {
        var item = FeedbackForecast.NewFeedbackForecast("code-987", "label-830");
        try
        {
            item.MoveTo(FeedbackForecastStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackForecastStatus.Closed, "status");
    }

    public static void FeedbackForecastRejectsMovesFromClosed()
    {
        var item = FeedbackForecast.NewFeedbackForecast("code-987", "label-830");
        item.Status = FeedbackForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackForecastStartsInDraft();
        FeedbackForecastMovesToClosed();
        FeedbackForecastRejectsMovesFromClosed();
    }
}
