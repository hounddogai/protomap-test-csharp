namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditReservationChecks
{
    public static void AuditReservationStartsInRequested()
    {
        var item = AuditReservation.NewAuditReservation("code-612", "title-227");
        Check.Equal(item.Status, AuditReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditReservationMovesToExpired()
    {
        var item = AuditReservation.NewAuditReservation("code-612", "title-227");
        try
        {
            item.MoveTo(AuditReservationStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditReservationStatus.Expired, "status");
    }

    public static void AuditReservationRejectsMovesFromExpired()
    {
        var item = AuditReservation.NewAuditReservation("code-612", "title-227");
        item.Status = AuditReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(AuditReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AuditReservationStartsInRequested();
        AuditReservationMovesToExpired();
        AuditReservationRejectsMovesFromExpired();
    }
}
