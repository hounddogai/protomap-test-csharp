namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackScheduleChecks
{
    public static void FeedbackScheduleStartsInProposed()
    {
        var item = FeedbackSchedule.NewFeedbackSchedule("code-352", "label-613");
        Check.Equal(item.Status, FeedbackScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackScheduleMovesToApproved()
    {
        var item = FeedbackSchedule.NewFeedbackSchedule("code-352", "label-613");
        try
        {
            item.MoveTo(FeedbackScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackScheduleStatus.Approved, "status");
    }

    public static void FeedbackScheduleRejectsMovesFromWithdrawn()
    {
        var item = FeedbackSchedule.NewFeedbackSchedule("code-352", "label-613");
        item.Status = FeedbackScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FeedbackScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FeedbackScheduleStartsInProposed();
        FeedbackScheduleMovesToApproved();
        FeedbackScheduleRejectsMovesFromWithdrawn();
    }
}
