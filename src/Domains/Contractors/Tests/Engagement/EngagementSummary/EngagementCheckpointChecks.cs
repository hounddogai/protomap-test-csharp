namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class EngagementCheckpointChecks
{
    public static void EngagementCheckpointStartsInProposed()
    {
        var item = EngagementCheckpoint.NewEngagementCheckpoint("code-873", "title-860");
        Check.Equal(item.Status, EngagementCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementCheckpointMovesToApproved()
    {
        var item = EngagementCheckpoint.NewEngagementCheckpoint("code-873", "title-860");
        try
        {
            item.MoveTo(EngagementCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementCheckpointStatus.Approved, "status");
    }

    public static void EngagementCheckpointRejectsMovesFromWithdrawn()
    {
        var item = EngagementCheckpoint.NewEngagementCheckpoint("code-873", "title-860");
        item.Status = EngagementCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementCheckpointStartsInProposed();
        EngagementCheckpointMovesToApproved();
        EngagementCheckpointRejectsMovesFromWithdrawn();
    }
}
