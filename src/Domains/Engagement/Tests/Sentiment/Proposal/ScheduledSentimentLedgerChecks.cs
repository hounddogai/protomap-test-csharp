namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ScheduledSentimentLedgerChecks
{
    public static void ScheduledSentimentLedgerStartsInNew()
    {
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("code-635", "label-225");
        Check.Equal(item.Status, ScheduledSentimentLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSentimentLedgerMovesToRunning()
    {
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("code-635", "label-225");
        try
        {
            item.MoveTo(ScheduledSentimentLedgerStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSentimentLedgerStatus.Running, "status");
    }

    public static void ScheduledSentimentLedgerRejectsMovesFromFailed()
    {
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("code-635", "label-225");
        item.Status = ScheduledSentimentLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledSentimentLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledSentimentLedgerStartsInNew();
        ScheduledSentimentLedgerMovesToRunning();
        ScheduledSentimentLedgerRejectsMovesFromFailed();
    }
}
