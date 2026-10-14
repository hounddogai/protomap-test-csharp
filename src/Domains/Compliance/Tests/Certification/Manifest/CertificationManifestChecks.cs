namespace Acme.Workforce.Compliance.Tests.Certification.Manifest;

using Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class CertificationManifestChecks
{
    public static void CertificationManifestStartsInPlanned()
    {
        var item = CertificationManifest.NewCertificationManifest("code-61", "label-60");
        Check.Equal(item.Status, CertificationManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationManifestMovesToInProgress()
    {
        var item = CertificationManifest.NewCertificationManifest("code-61", "label-60");
        try
        {
            item.MoveTo(CertificationManifestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationManifestStatus.InProgress, "status");
    }

    public static void CertificationManifestRejectsMovesFromCancelled()
    {
        var item = CertificationManifest.NewCertificationManifest("code-61", "label-60");
        item.Status = CertificationManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CertificationManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CertificationManifestStartsInPlanned();
        CertificationManifestMovesToInProgress();
        CertificationManifestRejectsMovesFromCancelled();
    }
}
