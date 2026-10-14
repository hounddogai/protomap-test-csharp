namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class SentimentBudgetChecks
{
    public static void SentimentBudgetStartsInIdle()
    {
        var item = SentimentBudget.NewSentimentBudget("code-162", "label-150");
        Check.Equal(item.Status, SentimentBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentBudgetMovesToWarming()
    {
        var item = SentimentBudget.NewSentimentBudget("code-162", "label-150");
        try
        {
            item.MoveTo(SentimentBudgetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentBudgetStatus.Warming, "status");
    }

    public static void SentimentBudgetRejectsMovesFromStopped()
    {
        var item = SentimentBudget.NewSentimentBudget("code-162", "label-150");
        item.Status = SentimentBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(SentimentBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SentimentBudgetStartsInIdle();
        SentimentBudgetMovesToWarming();
        SentimentBudgetRejectsMovesFromStopped();
    }
}
