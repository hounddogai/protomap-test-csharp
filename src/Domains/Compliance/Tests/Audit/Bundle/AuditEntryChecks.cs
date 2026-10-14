namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AuditEntryChecks
{
    public static void AuditEntryStartsInProposed()
    {
        var item = AuditEntry.NewAuditEntry("code-356", "label-61");
        Check.Equal(item.Status, AuditEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditEntryMovesToRejected()
    {
        var item = AuditEntry.NewAuditEntry("code-356", "label-61");
        try
        {
            item.MoveTo(AuditEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditEntryStatus.Rejected, "status");
    }

    public static void AuditEntryRejectsMovesFromWithdrawn()
    {
        var item = AuditEntry.NewAuditEntry("code-356", "label-61");
        item.Status = AuditEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AuditEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AuditEntryStartsInProposed();
        AuditEntryMovesToRejected();
        AuditEntryRejectsMovesFromWithdrawn();
    }
}
