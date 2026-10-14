namespace Acme.Workforce.Compliance.Tests.Certification.Snapshot;

using Acme.Workforce.Compliance.Certification.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredCertificationSnapshotChecks
{
    public static void TieredCertificationSnapshotStartsInRequested()
    {
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("code-167", "label-576");
        Check.Equal(item.Status, TieredCertificationSnapshotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCertificationSnapshotMovesToRevoked()
    {
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("code-167", "label-576");
        try
        {
            item.MoveTo(TieredCertificationSnapshotStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCertificationSnapshotStatus.Revoked, "status");
    }

    public static void TieredCertificationSnapshotRejectsMovesFromExpired()
    {
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("code-167", "label-576");
        item.Status = TieredCertificationSnapshotStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredCertificationSnapshotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredCertificationSnapshotStartsInRequested();
        TieredCertificationSnapshotMovesToRevoked();
        TieredCertificationSnapshotRejectsMovesFromExpired();
    }
}
