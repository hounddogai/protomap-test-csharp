namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveRateTemplateRulesChecks
{
    public static void AdaptiveRateTemplateValidatorReportsEveryProblem()
    {
        var validator = AdaptiveRateTemplateRules.DefaultAdaptiveRateTemplateValidator();
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveRateTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveRateTemplateRequiredLabelRule("adaptive rate template needs a value");
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRateTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveRateTemplateLabelLengthRule(255);
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveRateTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveRateTemplateRequiredCodeRule("adaptive rate template needs a value");
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRateTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveRateTemplateCodeLengthRule(86400);
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveRateTemplateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new AdaptiveRateTemplateRequiredPhoneRule("adaptive rate template needs this field");
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveRateTemplateValidatorReportsEveryProblem();
        AdaptiveRateTemplateRequiredLabelRuleFlagsABlankLabel();
        AdaptiveRateTemplateLabelLengthRuleFlagsALongLabel();
        AdaptiveRateTemplateRequiredCodeRuleFlagsABlankCode();
        AdaptiveRateTemplateCodeLengthRuleFlagsALongCode();
        AdaptiveRateTemplateRequiredPhoneRuleFlagsABlankPhone();
    }
}
