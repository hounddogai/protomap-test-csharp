namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackSnapshotChecks
{
    public static void FeedbackSnapshotStartsInProposed()
    {
        var item = FeedbackSnapshot.NewFeedbackSnapshot("code-629", "label-43");
        Check.Equal(item.Status, FeedbackSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackSnapshotMovesToApproved()
    {
        var item = FeedbackSnapshot.NewFeedbackSnapshot("code-629", "label-43");
        try
        {
            item.MoveTo(FeedbackSnapshotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackSnapshotStatus.Approved, "status");
    }

    public static void FeedbackSnapshotRejectsMovesFromWithdrawn()
    {
        var item = FeedbackSnapshot.NewFeedbackSnapshot("code-629", "label-43");
        item.Status = FeedbackSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FeedbackSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FeedbackSnapshotStartsInProposed();
        FeedbackSnapshotMovesToApproved();
        FeedbackSnapshotRejectsMovesFromWithdrawn();
    }
}
