namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class AttestationThresholdChecks
{
    public static void AttestationThresholdStartsInDraft()
    {
        var item = AttestationThreshold.NewAttestationThreshold("code-972", "name-614");
        Check.Equal(item.Status, AttestationThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationThresholdMovesToClosed()
    {
        var item = AttestationThreshold.NewAttestationThreshold("code-972", "name-614");
        try
        {
            item.MoveTo(AttestationThresholdStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationThresholdStatus.Closed, "status");
    }

    public static void AttestationThresholdRejectsMovesFromClosed()
    {
        var item = AttestationThreshold.NewAttestationThreshold("code-972", "name-614");
        item.Status = AttestationThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(AttestationThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AttestationThresholdStartsInDraft();
        AttestationThresholdMovesToClosed();
        AttestationThresholdRejectsMovesFromClosed();
    }
}
