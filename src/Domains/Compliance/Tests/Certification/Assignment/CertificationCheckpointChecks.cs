namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationCheckpointChecks
{
    public static void CertificationCheckpointStartsInDraft()
    {
        var item = CertificationCheckpoint.NewCertificationCheckpoint("code-649", "label-404");
        Check.Equal(item.Status, CertificationCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationCheckpointMovesToActive()
    {
        var item = CertificationCheckpoint.NewCertificationCheckpoint("code-649", "label-404");
        try
        {
            item.MoveTo(CertificationCheckpointStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationCheckpointStatus.Active, "status");
    }

    public static void CertificationCheckpointRejectsMovesFromClosed()
    {
        var item = CertificationCheckpoint.NewCertificationCheckpoint("code-649", "label-404");
        item.Status = CertificationCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(CertificationCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CertificationCheckpointStartsInDraft();
        CertificationCheckpointMovesToActive();
        CertificationCheckpointRejectsMovesFromClosed();
    }
}
