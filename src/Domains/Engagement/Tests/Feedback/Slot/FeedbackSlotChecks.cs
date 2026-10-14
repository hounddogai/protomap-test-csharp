namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FeedbackSlotChecks
{
    public static void FeedbackSlotStartsInPlanned()
    {
        var item = FeedbackSlot.NewFeedbackSlot("code-226", "name-877");
        Check.Equal(item.Status, FeedbackSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackSlotMovesToScheduled()
    {
        var item = FeedbackSlot.NewFeedbackSlot("code-226", "name-877");
        try
        {
            item.MoveTo(FeedbackSlotStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackSlotStatus.Scheduled, "status");
    }

    public static void FeedbackSlotRejectsMovesFromCancelled()
    {
        var item = FeedbackSlot.NewFeedbackSlot("code-226", "name-877");
        item.Status = FeedbackSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FeedbackSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FeedbackSlotStartsInPlanned();
        FeedbackSlotMovesToScheduled();
        FeedbackSlotRejectsMovesFromCancelled();
    }
}
