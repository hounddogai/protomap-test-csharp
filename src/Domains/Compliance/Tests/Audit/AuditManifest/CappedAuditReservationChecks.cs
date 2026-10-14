namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class CappedAuditReservationChecks
{
    public static void CappedAuditReservationStartsInProposed()
    {
        var item = CappedAuditReservation.NewCappedAuditReservation("code-212", "name-839");
        Check.Equal(item.Status, CappedAuditReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAuditReservationMovesToApproved()
    {
        var item = CappedAuditReservation.NewCappedAuditReservation("code-212", "name-839");
        try
        {
            item.MoveTo(CappedAuditReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAuditReservationStatus.Approved, "status");
    }

    public static void CappedAuditReservationRejectsMovesFromWithdrawn()
    {
        var item = CappedAuditReservation.NewCappedAuditReservation("code-212", "name-839");
        item.Status = CappedAuditReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedAuditReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedAuditReservationStartsInProposed();
        CappedAuditReservationMovesToApproved();
        CappedAuditReservationRejectsMovesFromWithdrawn();
    }
}
