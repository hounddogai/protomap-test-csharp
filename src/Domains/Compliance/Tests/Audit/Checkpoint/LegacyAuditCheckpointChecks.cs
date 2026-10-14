namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyAuditCheckpointChecks
{
    public static void LegacyAuditCheckpointStartsInNew()
    {
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("code-664", "title-96");
        Check.Equal(item.Status, LegacyAuditCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAuditCheckpointMovesToRunning()
    {
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("code-664", "title-96");
        try
        {
            item.MoveTo(LegacyAuditCheckpointStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAuditCheckpointStatus.Running, "status");
    }

    public static void LegacyAuditCheckpointRejectsMovesFromFailed()
    {
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("code-664", "title-96");
        item.Status = LegacyAuditCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyAuditCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyAuditCheckpointStartsInNew();
        LegacyAuditCheckpointMovesToRunning();
        LegacyAuditCheckpointRejectsMovesFromFailed();
    }
}
