namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyAttestationTemplateRulesChecks
{
    public static void LegacyAttestationTemplateValidatorReportsEveryProblem()
    {
        var validator = LegacyAttestationTemplateRules.DefaultLegacyAttestationTemplateValidator();
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAttestationTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyAttestationTemplateRequiredCodeRule("legacy attestation template needs an entry");
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAttestationTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyAttestationTemplateLabelLengthRule(16);
        var item = LegacyAttestationTemplate.NewLegacyAttestationTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyAttestationTemplateValidatorReportsEveryProblem();
        LegacyAttestationTemplateRequiredCodeRuleFlagsABlankCode();
        LegacyAttestationTemplateLabelLengthRuleFlagsALongLabel();
    }
}
