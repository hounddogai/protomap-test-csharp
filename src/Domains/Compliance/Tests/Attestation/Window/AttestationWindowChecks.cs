namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationWindowChecks
{
    public static void AttestationWindowStartsInRequested()
    {
        var item = AttestationWindow.NewAttestationWindow("code-18", "title-223");
        Check.Equal(item.Status, AttestationWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationWindowMovesToRevoked()
    {
        var item = AttestationWindow.NewAttestationWindow("code-18", "title-223");
        try
        {
            item.MoveTo(AttestationWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationWindowStatus.Revoked, "status");
    }

    public static void AttestationWindowRejectsMovesFromExpired()
    {
        var item = AttestationWindow.NewAttestationWindow("code-18", "title-223");
        item.Status = AttestationWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(AttestationWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AttestationWindowStartsInRequested();
        AttestationWindowMovesToRevoked();
        AttestationWindowRejectsMovesFromExpired();
    }
}
