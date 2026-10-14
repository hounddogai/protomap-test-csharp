namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentSummaryChecks
{
    public static void SentimentSummaryStartsInNew()
    {
        var item = SentimentSummary.NewSentimentSummary("code-835", "title-698");
        Check.Equal(item.Status, SentimentSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentSummaryMovesToQueued()
    {
        var item = SentimentSummary.NewSentimentSummary("code-835", "title-698");
        try
        {
            item.MoveTo(SentimentSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentSummaryStatus.Queued, "status");
    }

    public static void SentimentSummaryRejectsMovesFromFailed()
    {
        var item = SentimentSummary.NewSentimentSummary("code-835", "title-698");
        item.Status = SentimentSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(SentimentSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        SentimentSummaryStartsInNew();
        SentimentSummaryMovesToQueued();
        SentimentSummaryRejectsMovesFromFailed();
    }
}
