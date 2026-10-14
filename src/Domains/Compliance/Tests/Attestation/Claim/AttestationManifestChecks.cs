namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationManifestChecks
{
    public static void AttestationManifestStartsInPlanned()
    {
        var item = AttestationManifest.NewAttestationManifest("code-230", "label-850");
        Check.Equal(item.Status, AttestationManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationManifestMovesToScheduled()
    {
        var item = AttestationManifest.NewAttestationManifest("code-230", "label-850");
        try
        {
            item.MoveTo(AttestationManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationManifestStatus.Scheduled, "status");
    }

    public static void AttestationManifestRejectsMovesFromCancelled()
    {
        var item = AttestationManifest.NewAttestationManifest("code-230", "label-850");
        item.Status = AttestationManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AttestationManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AttestationManifestStartsInPlanned();
        AttestationManifestMovesToScheduled();
        AttestationManifestRejectsMovesFromCancelled();
    }
}
