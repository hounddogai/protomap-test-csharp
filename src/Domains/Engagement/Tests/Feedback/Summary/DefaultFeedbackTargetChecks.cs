namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultFeedbackTargetChecks
{
    public static void DefaultFeedbackTargetStartsInPlanned()
    {
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
        Check.Equal(item.Status, DefaultFeedbackTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultFeedbackTargetMovesToInProgress()
    {
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
        try
        {
            item.MoveTo(DefaultFeedbackTargetStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultFeedbackTargetStatus.InProgress, "status");
    }

    public static void DefaultFeedbackTargetRejectsMovesFromCancelled()
    {
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
        item.Status = DefaultFeedbackTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultFeedbackTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultFeedbackTargetStartsInPlanned();
        DefaultFeedbackTargetMovesToInProgress();
        DefaultFeedbackTargetRejectsMovesFromCancelled();
    }
}
