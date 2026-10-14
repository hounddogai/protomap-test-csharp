namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackMetricChecks
{
    public static void FeedbackMetricStartsInProposed()
    {
        var item = FeedbackMetric.NewFeedbackMetric("code-872", "title-274");
        Check.Equal(item.Status, FeedbackMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackMetricMovesToRejected()
    {
        var item = FeedbackMetric.NewFeedbackMetric("code-872", "title-274");
        try
        {
            item.MoveTo(FeedbackMetricStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackMetricStatus.Rejected, "status");
    }

    public static void FeedbackMetricRejectsMovesFromWithdrawn()
    {
        var item = FeedbackMetric.NewFeedbackMetric("code-872", "title-274");
        item.Status = FeedbackMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FeedbackMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FeedbackMetricStartsInProposed();
        FeedbackMetricMovesToRejected();
        FeedbackMetricRejectsMovesFromWithdrawn();
    }
}
