namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class AdaptiveFeedbackSummaryChecks
{
    public static void AdaptiveFeedbackSummaryStartsInPlanned()
    {
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("code-115", "title-965");
        Check.Equal(item.Status, AdaptiveFeedbackSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFeedbackSummaryMovesToInProgress()
    {
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("code-115", "title-965");
        try
        {
            item.MoveTo(AdaptiveFeedbackSummaryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFeedbackSummaryStatus.InProgress, "status");
    }

    public static void AdaptiveFeedbackSummaryRejectsMovesFromCancelled()
    {
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("code-115", "title-965");
        item.Status = AdaptiveFeedbackSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveFeedbackSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackSummaryStartsInPlanned();
        AdaptiveFeedbackSummaryMovesToInProgress();
        AdaptiveFeedbackSummaryRejectsMovesFromCancelled();
    }
}
