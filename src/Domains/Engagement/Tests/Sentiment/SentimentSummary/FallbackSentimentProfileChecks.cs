namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentProfileChecks
{
    public static void FallbackSentimentProfileStartsInProposed()
    {
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("code-4", "title-374");
        Check.Equal(item.Status, FallbackSentimentProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSentimentProfileMovesToApproved()
    {
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("code-4", "title-374");
        try
        {
            item.MoveTo(FallbackSentimentProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSentimentProfileStatus.Approved, "status");
    }

    public static void FallbackSentimentProfileRejectsMovesFromWithdrawn()
    {
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("code-4", "title-374");
        item.Status = FallbackSentimentProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackSentimentProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackSentimentProfileStartsInProposed();
        FallbackSentimentProfileMovesToApproved();
        FallbackSentimentProfileRejectsMovesFromWithdrawn();
    }
}
