namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentQuotaChecks
{
    public static void SentimentQuotaStartsInIdle()
    {
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
        Check.Equal(item.Status, SentimentQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentQuotaMovesToWarming()
    {
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
        try
        {
            item.MoveTo(SentimentQuotaStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentQuotaStatus.Warming, "status");
    }

    public static void SentimentQuotaRejectsMovesFromStopped()
    {
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
        item.Status = SentimentQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(SentimentQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SentimentQuotaStartsInIdle();
        SentimentQuotaMovesToWarming();
        SentimentQuotaRejectsMovesFromStopped();
    }
}
