namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentMappingChecks
{
    public static void SentimentMappingStartsInPlanned()
    {
        var item = SentimentMapping.NewSentimentMapping("code-715", "title-167");
        Check.Equal(item.Status, SentimentMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentMappingMovesToScheduled()
    {
        var item = SentimentMapping.NewSentimentMapping("code-715", "title-167");
        try
        {
            item.MoveTo(SentimentMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentMappingStatus.Scheduled, "status");
    }

    public static void SentimentMappingRejectsMovesFromCancelled()
    {
        var item = SentimentMapping.NewSentimentMapping("code-715", "title-167");
        item.Status = SentimentMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SentimentMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SentimentMappingStartsInPlanned();
        SentimentMappingMovesToScheduled();
        SentimentMappingRejectsMovesFromCancelled();
    }
}
