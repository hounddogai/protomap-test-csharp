namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackMilestoneChecks
{
    public static void FeedbackMilestoneStartsInIdle()
    {
        var item = FeedbackMilestone.NewFeedbackMilestone("code-416", "title-321");
        Check.Equal(item.Status, FeedbackMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackMilestoneMovesToWarming()
    {
        var item = FeedbackMilestone.NewFeedbackMilestone("code-416", "title-321");
        try
        {
            item.MoveTo(FeedbackMilestoneStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackMilestoneStatus.Warming, "status");
    }

    public static void FeedbackMilestoneRejectsMovesFromStopped()
    {
        var item = FeedbackMilestone.NewFeedbackMilestone("code-416", "title-321");
        item.Status = FeedbackMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(FeedbackMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FeedbackMilestoneStartsInIdle();
        FeedbackMilestoneMovesToWarming();
        FeedbackMilestoneRejectsMovesFromStopped();
    }
}
