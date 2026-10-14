namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneChecks
{
    public static void SentimentMilestoneStartsInRequested()
    {
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
        Check.Equal(item.Status, SentimentMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentMilestoneMovesToRevoked()
    {
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
        try
        {
            item.MoveTo(SentimentMilestoneStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentMilestoneStatus.Revoked, "status");
    }

    public static void SentimentMilestoneRejectsMovesFromExpired()
    {
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
        item.Status = SentimentMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentMilestoneStartsInRequested();
        SentimentMilestoneMovesToRevoked();
        SentimentMilestoneRejectsMovesFromExpired();
    }
}
