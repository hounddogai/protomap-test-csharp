namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PendingEngagementCheckpointChecks
{
    public static void PendingEngagementCheckpointStartsInProposed()
    {
        var item = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint("code-302", "title-127");
        Check.Equal(item.Status, PendingEngagementCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingEngagementCheckpointMovesToRejected()
    {
        var item = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint("code-302", "title-127");
        try
        {
            item.MoveTo(PendingEngagementCheckpointStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingEngagementCheckpointStatus.Rejected, "status");
    }

    public static void PendingEngagementCheckpointRejectsMovesFromWithdrawn()
    {
        var item = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint("code-302", "title-127");
        item.Status = PendingEngagementCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingEngagementCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingEngagementCheckpointStartsInProposed();
        PendingEngagementCheckpointMovesToRejected();
        PendingEngagementCheckpointRejectsMovesFromWithdrawn();
    }
}
