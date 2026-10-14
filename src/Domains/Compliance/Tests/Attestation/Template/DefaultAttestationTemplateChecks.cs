namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class DefaultAttestationTemplateChecks
{
    public static void DefaultAttestationTemplateStartsInCreated()
    {
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("code-673", "title-971");
        Check.Equal(item.Status, DefaultAttestationTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAttestationTemplateMovesToValidated()
    {
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("code-673", "title-971");
        try
        {
            item.MoveTo(DefaultAttestationTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAttestationTemplateStatus.Validated, "status");
    }

    public static void DefaultAttestationTemplateRejectsMovesFromRetired()
    {
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("code-673", "title-971");
        item.Status = DefaultAttestationTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultAttestationTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultAttestationTemplateStartsInCreated();
        DefaultAttestationTemplateMovesToValidated();
        DefaultAttestationTemplateRejectsMovesFromRetired();
    }
}
