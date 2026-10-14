namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class PendingAttestationThresholdChecks
{
    public static void PendingAttestationThresholdStartsInDraft()
    {
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("code-246", "label-184");
        Check.Equal(item.Status, PendingAttestationThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAttestationThresholdMovesToSuspended()
    {
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("code-246", "label-184");
        try
        {
            item.MoveTo(PendingAttestationThresholdStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAttestationThresholdStatus.Suspended, "status");
    }

    public static void PendingAttestationThresholdRejectsMovesFromClosed()
    {
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("code-246", "label-184");
        item.Status = PendingAttestationThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingAttestationThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingAttestationThresholdStartsInDraft();
        PendingAttestationThresholdMovesToSuspended();
        PendingAttestationThresholdRejectsMovesFromClosed();
    }
}
