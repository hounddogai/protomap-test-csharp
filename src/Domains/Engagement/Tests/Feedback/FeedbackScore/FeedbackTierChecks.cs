namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackTierChecks
{
    public static void FeedbackTierStartsInNew()
    {
        var item = FeedbackTier.NewFeedbackTier("code-421", "title-885");
        Check.Equal(item.Status, FeedbackTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackTierMovesToDone()
    {
        var item = FeedbackTier.NewFeedbackTier("code-421", "title-885");
        try
        {
            item.MoveTo(FeedbackTierStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackTierStatus.Done, "status");
    }

    public static void FeedbackTierRejectsMovesFromFailed()
    {
        var item = FeedbackTier.NewFeedbackTier("code-421", "title-885");
        item.Status = FeedbackTierStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackTierStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackTierStartsInNew();
        FeedbackTierMovesToDone();
        FeedbackTierRejectsMovesFromFailed();
    }
}
