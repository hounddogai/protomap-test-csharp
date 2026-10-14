namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestChecks
{
    public static void SentimentDigestStartsInRequested()
    {
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
        Check.Equal(item.Status, SentimentDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentDigestMovesToExpired()
    {
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
        try
        {
            item.MoveTo(SentimentDigestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentDigestStatus.Expired, "status");
    }

    public static void SentimentDigestRejectsMovesFromExpired()
    {
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
        item.Status = SentimentDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentDigestStartsInRequested();
        SentimentDigestMovesToExpired();
        SentimentDigestRejectsMovesFromExpired();
    }
}
