namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class AuditLedgerChecks
{
    public static void AuditLedgerStartsInOpen()
    {
        var item = AuditLedger.NewAuditLedger("code-298", "name-204");
        Check.Equal(item.Status, AuditLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditLedgerMovesToArchived()
    {
        var item = AuditLedger.NewAuditLedger("code-298", "name-204");
        try
        {
            item.MoveTo(AuditLedgerStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditLedgerStatus.Archived, "status");
    }

    public static void AuditLedgerRejectsMovesFromArchived()
    {
        var item = AuditLedger.NewAuditLedger("code-298", "name-204");
        item.Status = AuditLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(AuditLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        AuditLedgerStartsInOpen();
        AuditLedgerMovesToArchived();
        AuditLedgerRejectsMovesFromArchived();
    }
}
