namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FeedbackTargetChecks
{
    public static void FeedbackTargetStartsInNew()
    {
        var item = FeedbackTarget.NewFeedbackTarget("code-839", "title-876");
        Check.Equal(item.Status, FeedbackTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackTargetMovesToQueued()
    {
        var item = FeedbackTarget.NewFeedbackTarget("code-839", "title-876");
        try
        {
            item.MoveTo(FeedbackTargetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackTargetStatus.Queued, "status");
    }

    public static void FeedbackTargetRejectsMovesFromFailed()
    {
        var item = FeedbackTarget.NewFeedbackTarget("code-839", "title-876");
        item.Status = FeedbackTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackTargetStartsInNew();
        FeedbackTargetMovesToQueued();
        FeedbackTargetRejectsMovesFromFailed();
    }
}
