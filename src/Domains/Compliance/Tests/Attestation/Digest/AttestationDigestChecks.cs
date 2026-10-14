namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationDigestChecks
{
    public static void AttestationDigestStartsInNew()
    {
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
        Check.Equal(item.Status, AttestationDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationDigestMovesToRunning()
    {
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
        try
        {
            item.MoveTo(AttestationDigestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationDigestStatus.Running, "status");
    }

    public static void AttestationDigestRejectsMovesFromFailed()
    {
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
        item.Status = AttestationDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(AttestationDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        AttestationDigestStartsInNew();
        AttestationDigestMovesToRunning();
        AttestationDigestRejectsMovesFromFailed();
    }
}
