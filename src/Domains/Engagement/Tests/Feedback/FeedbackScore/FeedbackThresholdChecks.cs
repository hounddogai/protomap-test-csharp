namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackThresholdChecks
{
    public static void FeedbackThresholdStartsInDraft()
    {
        var item = FeedbackThreshold.NewFeedbackThreshold("code-493", "label-298");
        Check.Equal(item.Status, FeedbackThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackThresholdMovesToPending()
    {
        var item = FeedbackThreshold.NewFeedbackThreshold("code-493", "label-298");
        try
        {
            item.MoveTo(FeedbackThresholdStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackThresholdStatus.Pending, "status");
    }

    public static void FeedbackThresholdRejectsMovesFromClosed()
    {
        var item = FeedbackThreshold.NewFeedbackThreshold("code-493", "label-298");
        item.Status = FeedbackThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackThresholdStartsInDraft();
        FeedbackThresholdMovesToPending();
        FeedbackThresholdRejectsMovesFromClosed();
    }
}
