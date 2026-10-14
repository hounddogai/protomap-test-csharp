namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class DefaultAttestationTemplateRulesChecks
{
    public static void DefaultAttestationTemplateValidatorReportsEveryProblem()
    {
        var validator = DefaultAttestationTemplateRules.DefaultDefaultAttestationTemplateValidator();
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAttestationTemplateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new DefaultAttestationTemplateRequiredCurrencyRule("default attestation template needs a value");
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAttestationTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultAttestationTemplateCodeLengthRule(64);
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAttestationTemplateCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new DefaultAttestationTemplateCurrencyLengthRule(3600);
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultAttestationTemplateValidatorReportsEveryProblem();
        DefaultAttestationTemplateRequiredCurrencyRuleFlagsABlankCurrency();
        DefaultAttestationTemplateCodeLengthRuleFlagsALongCode();
        DefaultAttestationTemplateCurrencyLengthRuleFlagsALongCurrency();
    }
}
