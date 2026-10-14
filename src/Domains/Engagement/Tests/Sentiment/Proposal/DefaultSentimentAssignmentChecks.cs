namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultSentimentAssignmentChecks
{
    public static void DefaultSentimentAssignmentStartsInRequested()
    {
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("code-819", "title-577");
        Check.Equal(item.Status, DefaultSentimentAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSentimentAssignmentMovesToRevoked()
    {
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("code-819", "title-577");
        try
        {
            item.MoveTo(DefaultSentimentAssignmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSentimentAssignmentStatus.Revoked, "status");
    }

    public static void DefaultSentimentAssignmentRejectsMovesFromExpired()
    {
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("code-819", "title-577");
        item.Status = DefaultSentimentAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultSentimentAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultSentimentAssignmentStartsInRequested();
        DefaultSentimentAssignmentMovesToRevoked();
        DefaultSentimentAssignmentRejectsMovesFromExpired();
    }
}
