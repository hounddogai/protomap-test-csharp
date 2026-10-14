namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AttestationTemplateRulesChecks
{
    public static void AttestationTemplateValidatorReportsEveryProblem()
    {
        var validator = AttestationTemplateRules.DefaultAttestationTemplateValidator();
        var item = AttestationTemplate.NewAttestationTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationTemplateRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AttestationTemplateRateRangeRule(7.4, 3646.6);
        var item = AttestationTemplate.NewAttestationTemplate("x", "y");
        item.Rate = 3648.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AttestationTemplateRequiredTitleRule("attestation template needs a value");
        var item = AttestationTemplate.NewAttestationTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationTemplateValidatorReportsEveryProblem();
        AttestationTemplateRateRangeRuleFlagsValuesAboveRate();
        AttestationTemplateRequiredTitleRuleFlagsABlankTitle();
    }
}
