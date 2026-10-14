namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentScheduleViewChecks
{
    public static void SentimentScheduleViewShowsTheStatusInCapitals()
    {
        var item = SentimentSchedule.NewSentimentSchedule("code-857", "name-635");
        var view = SentimentScheduleView.ToSentimentScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentScheduleView.ToSentimentScheduleViews(new List<SentimentSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentScheduleViewShowsTheStatusInCapitals();
    }
}
