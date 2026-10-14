namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class CertificationReservationChecks
{
    public static void CertificationReservationStartsInIdle()
    {
        var item = CertificationReservation.NewCertificationReservation("code-722", "label-127");
        Check.Equal(item.Status, CertificationReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationReservationMovesToReady()
    {
        var item = CertificationReservation.NewCertificationReservation("code-722", "label-127");
        try
        {
            item.MoveTo(CertificationReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationReservationStatus.Ready, "status");
    }

    public static void CertificationReservationRejectsMovesFromStopped()
    {
        var item = CertificationReservation.NewCertificationReservation("code-722", "label-127");
        item.Status = CertificationReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(CertificationReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CertificationReservationStartsInIdle();
        CertificationReservationMovesToReady();
        CertificationReservationRejectsMovesFromStopped();
    }
}
