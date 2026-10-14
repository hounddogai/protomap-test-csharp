namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyChecks
{
    public static void AttestationPolicyStartsInIdle()
    {
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
        Check.Equal(item.Status, AttestationPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationPolicyMovesToWarming()
    {
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
        try
        {
            item.MoveTo(AttestationPolicyStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationPolicyStatus.Warming, "status");
    }

    public static void AttestationPolicyRejectsMovesFromStopped()
    {
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
        item.Status = AttestationPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(AttestationPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AttestationPolicyStartsInIdle();
        AttestationPolicyMovesToWarming();
        AttestationPolicyRejectsMovesFromStopped();
    }
}
