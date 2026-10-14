namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AttestationTierChecks
{
    public static void AttestationTierStartsInOpen()
    {
        var item = AttestationTier.NewAttestationTier("code-309", "label-465");
        Check.Equal(item.Status, AttestationTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationTierMovesToArchived()
    {
        var item = AttestationTier.NewAttestationTier("code-309", "label-465");
        try
        {
            item.MoveTo(AttestationTierStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationTierStatus.Archived, "status");
    }

    public static void AttestationTierRejectsMovesFromArchived()
    {
        var item = AttestationTier.NewAttestationTier("code-309", "label-465");
        item.Status = AttestationTierStatus.Archived;
        Check.Fails(() => item.MoveTo(AttestationTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        AttestationTierStartsInOpen();
        AttestationTierMovesToArchived();
        AttestationTierRejectsMovesFromArchived();
    }
}
