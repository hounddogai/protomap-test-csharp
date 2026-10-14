namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientSentimentQuotaChecks
{
    public static void LenientSentimentQuotaStartsInNew()
    {
        var item = LenientSentimentQuota.NewLenientSentimentQuota("code-66", "title-774");
        Check.Equal(item.Status, LenientSentimentQuotaStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSentimentQuotaMovesToRunning()
    {
        var item = LenientSentimentQuota.NewLenientSentimentQuota("code-66", "title-774");
        try
        {
            item.MoveTo(LenientSentimentQuotaStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSentimentQuotaStatus.Running, "status");
    }

    public static void LenientSentimentQuotaRejectsMovesFromFailed()
    {
        var item = LenientSentimentQuota.NewLenientSentimentQuota("code-66", "title-774");
        item.Status = LenientSentimentQuotaStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientSentimentQuotaStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientSentimentQuotaStartsInNew();
        LenientSentimentQuotaMovesToRunning();
        LenientSentimentQuotaRejectsMovesFromFailed();
    }
}
