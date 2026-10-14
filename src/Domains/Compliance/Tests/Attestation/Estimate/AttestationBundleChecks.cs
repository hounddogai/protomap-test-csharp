namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationBundleChecks
{
    public static void AttestationBundleStartsInRequested()
    {
        var item = AttestationBundle.NewAttestationBundle("code-816", "name-823");
        Check.Equal(item.Status, AttestationBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationBundleMovesToRevoked()
    {
        var item = AttestationBundle.NewAttestationBundle("code-816", "name-823");
        try
        {
            item.MoveTo(AttestationBundleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationBundleStatus.Revoked, "status");
    }

    public static void AttestationBundleRejectsMovesFromExpired()
    {
        var item = AttestationBundle.NewAttestationBundle("code-816", "name-823");
        item.Status = AttestationBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(AttestationBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AttestationBundleStartsInRequested();
        AttestationBundleMovesToRevoked();
        AttestationBundleRejectsMovesFromExpired();
    }
}
