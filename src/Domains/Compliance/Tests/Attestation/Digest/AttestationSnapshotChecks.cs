namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationSnapshotChecks
{
    public static void AttestationSnapshotStartsInIdle()
    {
        var item = AttestationSnapshot.NewAttestationSnapshot("code-423", "label-98");
        Check.Equal(item.Status, AttestationSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationSnapshotMovesToStopped()
    {
        var item = AttestationSnapshot.NewAttestationSnapshot("code-423", "label-98");
        try
        {
            item.MoveTo(AttestationSnapshotStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationSnapshotStatus.Stopped, "status");
    }

    public static void AttestationSnapshotRejectsMovesFromStopped()
    {
        var item = AttestationSnapshot.NewAttestationSnapshot("code-423", "label-98");
        item.Status = AttestationSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(AttestationSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AttestationSnapshotStartsInIdle();
        AttestationSnapshotMovesToStopped();
        AttestationSnapshotRejectsMovesFromStopped();
    }
}
