namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackAllocationChecks
{
    public static void FeedbackAllocationStartsInPlanned()
    {
        var item = FeedbackAllocation.NewFeedbackAllocation("code-996", "title-266");
        Check.Equal(item.Status, FeedbackAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackAllocationMovesToScheduled()
    {
        var item = FeedbackAllocation.NewFeedbackAllocation("code-996", "title-266");
        try
        {
            item.MoveTo(FeedbackAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackAllocationStatus.Scheduled, "status");
    }

    public static void FeedbackAllocationRejectsMovesFromCancelled()
    {
        var item = FeedbackAllocation.NewFeedbackAllocation("code-996", "title-266");
        item.Status = FeedbackAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FeedbackAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FeedbackAllocationStartsInPlanned();
        FeedbackAllocationMovesToScheduled();
        FeedbackAllocationRejectsMovesFromCancelled();
    }
}
