namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentClaimChecks
{
    public static void SentimentClaimStartsInProposed()
    {
        var item = SentimentClaim.NewSentimentClaim("code-205", "title-722");
        Check.Equal(item.Status, SentimentClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentClaimMovesToApproved()
    {
        var item = SentimentClaim.NewSentimentClaim("code-205", "title-722");
        try
        {
            item.MoveTo(SentimentClaimStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentClaimStatus.Approved, "status");
    }

    public static void SentimentClaimRejectsMovesFromWithdrawn()
    {
        var item = SentimentClaim.NewSentimentClaim("code-205", "title-722");
        item.Status = SentimentClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SentimentClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SentimentClaimStartsInProposed();
        SentimentClaimMovesToApproved();
        SentimentClaimRejectsMovesFromWithdrawn();
    }
}
