namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingChecks
{
    public static void AttestationMappingStartsInProposed()
    {
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
        Check.Equal(item.Status, AttestationMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationMappingMovesToWithdrawn()
    {
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
        try
        {
            item.MoveTo(AttestationMappingStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationMappingStatus.Withdrawn, "status");
    }

    public static void AttestationMappingRejectsMovesFromWithdrawn()
    {
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
        item.Status = AttestationMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AttestationMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AttestationMappingStartsInProposed();
        AttestationMappingMovesToWithdrawn();
        AttestationMappingRejectsMovesFromWithdrawn();
    }
}
