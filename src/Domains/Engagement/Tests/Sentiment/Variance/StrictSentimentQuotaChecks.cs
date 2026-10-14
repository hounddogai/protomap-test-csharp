namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictSentimentQuotaChecks
{
    public static void StrictSentimentQuotaStartsInProposed()
    {
        var item = StrictSentimentQuota.NewStrictSentimentQuota("code-661", "label-631");
        Check.Equal(item.Status, StrictSentimentQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSentimentQuotaMovesToRejected()
    {
        var item = StrictSentimentQuota.NewStrictSentimentQuota("code-661", "label-631");
        try
        {
            item.MoveTo(StrictSentimentQuotaStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSentimentQuotaStatus.Rejected, "status");
    }

    public static void StrictSentimentQuotaRejectsMovesFromWithdrawn()
    {
        var item = StrictSentimentQuota.NewStrictSentimentQuota("code-661", "label-631");
        item.Status = StrictSentimentQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictSentimentQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictSentimentQuotaStartsInProposed();
        StrictSentimentQuotaMovesToRejected();
        StrictSentimentQuotaRejectsMovesFromWithdrawn();
    }
}
