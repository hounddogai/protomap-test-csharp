namespace Acme.Workforce.Engagement.Tests.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledFeedbackBaselineChecks
{
    public static void ScheduledFeedbackBaselineStartsInOpen()
    {
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
        Check.Equal(item.Status, ScheduledFeedbackBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFeedbackBaselineMovesToInReview()
    {
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
        try
        {
            item.MoveTo(ScheduledFeedbackBaselineStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFeedbackBaselineStatus.InReview, "status");
    }

    public static void ScheduledFeedbackBaselineRejectsMovesFromArchived()
    {
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
        item.Status = ScheduledFeedbackBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledFeedbackBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledFeedbackBaselineStartsInOpen();
        ScheduledFeedbackBaselineMovesToInReview();
        ScheduledFeedbackBaselineRejectsMovesFromArchived();
    }
}
