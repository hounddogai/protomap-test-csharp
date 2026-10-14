namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackPolicyChecks
{
    public static void FeedbackPolicyStartsInPlanned()
    {
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
        Check.Equal(item.Status, FeedbackPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackPolicyMovesToInProgress()
    {
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
        try
        {
            item.MoveTo(FeedbackPolicyStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackPolicyStatus.InProgress, "status");
    }

    public static void FeedbackPolicyRejectsMovesFromCancelled()
    {
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
        item.Status = FeedbackPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FeedbackPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FeedbackPolicyStartsInPlanned();
        FeedbackPolicyMovesToInProgress();
        FeedbackPolicyRejectsMovesFromCancelled();
    }
}
