namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentScheduleChecks
{
    public static void SentimentScheduleStartsInOpen()
    {
        var item = SentimentSchedule.NewSentimentSchedule("code-857", "name-635");
        Check.Equal(item.Status, SentimentScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentScheduleMovesToArchived()
    {
        var item = SentimentSchedule.NewSentimentSchedule("code-857", "name-635");
        try
        {
            item.MoveTo(SentimentScheduleStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentScheduleStatus.Archived, "status");
    }

    public static void SentimentScheduleRejectsMovesFromArchived()
    {
        var item = SentimentSchedule.NewSentimentSchedule("code-857", "name-635");
        item.Status = SentimentScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(SentimentScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        SentimentScheduleStartsInOpen();
        SentimentScheduleMovesToArchived();
        SentimentScheduleRejectsMovesFromArchived();
    }
}
