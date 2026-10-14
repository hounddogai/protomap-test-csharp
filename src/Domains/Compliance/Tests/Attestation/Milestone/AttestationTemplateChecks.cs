namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AttestationTemplateChecks
{
    public static void AttestationTemplateStartsInRequested()
    {
        var item = AttestationTemplate.NewAttestationTemplate("code-293", "title-275");
        Check.Equal(item.Status, AttestationTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationTemplateMovesToExpired()
    {
        var item = AttestationTemplate.NewAttestationTemplate("code-293", "title-275");
        try
        {
            item.MoveTo(AttestationTemplateStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationTemplateStatus.Expired, "status");
    }

    public static void AttestationTemplateRejectsMovesFromExpired()
    {
        var item = AttestationTemplate.NewAttestationTemplate("code-293", "title-275");
        item.Status = AttestationTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(AttestationTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AttestationTemplateStartsInRequested();
        AttestationTemplateMovesToExpired();
        AttestationTemplateRejectsMovesFromExpired();
    }
}
