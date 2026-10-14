namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyAttestationTemplateChecks
{
    public static void LegacyAttestationTemplateStartsInOpen()
    {
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("code-580", "name-8");
        Check.Equal(item.Status, LegacyAttestationTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAttestationTemplateMovesToReopened()
    {
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("code-580", "name-8");
        try
        {
            item.MoveTo(LegacyAttestationTemplateStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAttestationTemplateStatus.Reopened, "status");
    }

    public static void LegacyAttestationTemplateRejectsMovesFromArchived()
    {
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("code-580", "name-8");
        item.Status = LegacyAttestationTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyAttestationTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyAttestationTemplateStartsInOpen();
        LegacyAttestationTemplateMovesToReopened();
        LegacyAttestationTemplateRejectsMovesFromArchived();
    }
}
