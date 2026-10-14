namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAllocationChecks
{
    public static void SentimentAllocationStartsInNew()
    {
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
        Check.Equal(item.Status, SentimentAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentAllocationMovesToDone()
    {
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
        try
        {
            item.MoveTo(SentimentAllocationStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentAllocationStatus.Done, "status");
    }

    public static void SentimentAllocationRejectsMovesFromFailed()
    {
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
        item.Status = SentimentAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(SentimentAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        SentimentAllocationStartsInNew();
        SentimentAllocationMovesToDone();
        SentimentAllocationRejectsMovesFromFailed();
    }
}
