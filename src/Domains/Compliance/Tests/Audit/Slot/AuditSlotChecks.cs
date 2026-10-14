namespace Acme.Workforce.Compliance.Tests.Audit.Slot;

using Acme.Workforce.Compliance.Audit.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AuditSlotChecks
{
    public static void AuditSlotStartsInProposed()
    {
        var item = AuditSlot.NewAuditSlot("code-469", "title-825");
        Check.Equal(item.Status, AuditSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditSlotMovesToRejected()
    {
        var item = AuditSlot.NewAuditSlot("code-469", "title-825");
        try
        {
            item.MoveTo(AuditSlotStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditSlotStatus.Rejected, "status");
    }

    public static void AuditSlotRejectsMovesFromWithdrawn()
    {
        var item = AuditSlot.NewAuditSlot("code-469", "title-825");
        item.Status = AuditSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AuditSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AuditSlotStartsInProposed();
        AuditSlotMovesToRejected();
        AuditSlotRejectsMovesFromWithdrawn();
    }
}
