namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalAuditCheckpointChecks
{
    public static void GlobalAuditCheckpointStartsInOpen()
    {
        var item = GlobalAuditCheckpoint.NewGlobalAuditCheckpoint("code-745", "name-544");
        Check.Equal(item.Status, GlobalAuditCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAuditCheckpointMovesToInReview()
    {
        var item = GlobalAuditCheckpoint.NewGlobalAuditCheckpoint("code-745", "name-544");
        try
        {
            item.MoveTo(GlobalAuditCheckpointStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAuditCheckpointStatus.InReview, "status");
    }

    public static void GlobalAuditCheckpointRejectsMovesFromArchived()
    {
        var item = GlobalAuditCheckpoint.NewGlobalAuditCheckpoint("code-745", "name-544");
        item.Status = GlobalAuditCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalAuditCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalAuditCheckpointStartsInOpen();
        GlobalAuditCheckpointMovesToInReview();
        GlobalAuditCheckpointRejectsMovesFromArchived();
    }
}
