namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SentimentBaselineChecks
{
    public static void SentimentBaselineStartsInProposed()
    {
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
        Check.Equal(item.Status, SentimentBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentBaselineMovesToApproved()
    {
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
        try
        {
            item.MoveTo(SentimentBaselineStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentBaselineStatus.Approved, "status");
    }

    public static void SentimentBaselineRejectsMovesFromWithdrawn()
    {
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
        item.Status = SentimentBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SentimentBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SentimentBaselineStartsInProposed();
        SentimentBaselineMovesToApproved();
        SentimentBaselineRejectsMovesFromWithdrawn();
    }
}
