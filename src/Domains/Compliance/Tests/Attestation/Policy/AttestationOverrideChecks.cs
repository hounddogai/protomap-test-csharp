namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationOverrideChecks
{
    public static void AttestationOverrideStartsInCreated()
    {
        var item = AttestationOverride.NewAttestationOverride("code-413", "label-847");
        Check.Equal(item.Status, AttestationOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationOverrideMovesToRetired()
    {
        var item = AttestationOverride.NewAttestationOverride("code-413", "label-847");
        try
        {
            item.MoveTo(AttestationOverrideStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationOverrideStatus.Retired, "status");
    }

    public static void AttestationOverrideRejectsMovesFromRetired()
    {
        var item = AttestationOverride.NewAttestationOverride("code-413", "label-847");
        item.Status = AttestationOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(AttestationOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        AttestationOverrideStartsInCreated();
        AttestationOverrideMovesToRetired();
        AttestationOverrideRejectsMovesFromRetired();
    }
}
