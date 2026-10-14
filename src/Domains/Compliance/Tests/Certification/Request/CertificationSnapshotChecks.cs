namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationSnapshotChecks
{
    public static void CertificationSnapshotStartsInRequested()
    {
        var item = CertificationSnapshot.NewCertificationSnapshot("code-42", "label-390");
        Check.Equal(item.Status, CertificationSnapshotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationSnapshotMovesToGranted()
    {
        var item = CertificationSnapshot.NewCertificationSnapshot("code-42", "label-390");
        try
        {
            item.MoveTo(CertificationSnapshotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationSnapshotStatus.Granted, "status");
    }

    public static void CertificationSnapshotRejectsMovesFromExpired()
    {
        var item = CertificationSnapshot.NewCertificationSnapshot("code-42", "label-390");
        item.Status = CertificationSnapshotStatus.Expired;
        Check.Fails(() => item.MoveTo(CertificationSnapshotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CertificationSnapshotStartsInRequested();
        CertificationSnapshotMovesToGranted();
        CertificationSnapshotRejectsMovesFromExpired();
    }
}
