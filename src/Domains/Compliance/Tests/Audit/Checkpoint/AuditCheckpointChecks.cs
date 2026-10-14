namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditCheckpointChecks
{
    public static void AuditCheckpointStartsInPlanned()
    {
        var item = AuditCheckpoint.NewAuditCheckpoint("code-551", "name-827");
        Check.Equal(item.Status, AuditCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditCheckpointMovesToInProgress()
    {
        var item = AuditCheckpoint.NewAuditCheckpoint("code-551", "name-827");
        try
        {
            item.MoveTo(AuditCheckpointStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditCheckpointStatus.InProgress, "status");
    }

    public static void AuditCheckpointRejectsMovesFromCancelled()
    {
        var item = AuditCheckpoint.NewAuditCheckpoint("code-551", "name-827");
        item.Status = AuditCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditCheckpointStartsInPlanned();
        AuditCheckpointMovesToInProgress();
        AuditCheckpointRejectsMovesFromCancelled();
    }
}
