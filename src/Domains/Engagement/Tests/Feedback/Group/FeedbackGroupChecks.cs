namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupChecks
{
    public static void FeedbackGroupStartsInIdle()
    {
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
        Check.Equal(item.Status, FeedbackGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackGroupMovesToStopped()
    {
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
        try
        {
            item.MoveTo(FeedbackGroupStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackGroupStatus.Stopped, "status");
    }

    public static void FeedbackGroupRejectsMovesFromStopped()
    {
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
        item.Status = FeedbackGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(FeedbackGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FeedbackGroupStartsInIdle();
        FeedbackGroupMovesToStopped();
        FeedbackGroupRejectsMovesFromStopped();
    }
}
