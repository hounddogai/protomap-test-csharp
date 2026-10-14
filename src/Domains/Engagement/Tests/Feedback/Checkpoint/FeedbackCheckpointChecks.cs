namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FeedbackCheckpointChecks
{
    public static void FeedbackCheckpointStartsInDraft()
    {
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
        Check.Equal(item.Status, FeedbackCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackCheckpointMovesToSuspended()
    {
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
        try
        {
            item.MoveTo(FeedbackCheckpointStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackCheckpointStatus.Suspended, "status");
    }

    public static void FeedbackCheckpointRejectsMovesFromClosed()
    {
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
        item.Status = FeedbackCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackCheckpointStartsInDraft();
        FeedbackCheckpointMovesToSuspended();
        FeedbackCheckpointRejectsMovesFromClosed();
    }
}
