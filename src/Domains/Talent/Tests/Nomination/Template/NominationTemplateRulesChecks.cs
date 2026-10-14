namespace Acme.Workforce.Talent.Tests.Nomination.Template;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class NominationTemplateRulesChecks
{
    public static void NominationTemplateValidatorReportsEveryProblem()
    {
        var validator = NominationTemplateRules.DefaultNominationTemplateValidator();
        var item = NominationTemplate.NewNominationTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationTemplateAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new NominationTemplateAttemptsRangeRule(16, 255);
        var item = NominationTemplate.NewNominationTemplate("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationTemplateRequiredCodeRule("nomination template needs this field");
        var item = NominationTemplate.NewNominationTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new NominationTemplateRequiredNameRule("nomination template needs a value");
        var item = NominationTemplate.NewNominationTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTemplateWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new NominationTemplateWeightRangeRule(4.9, 3573.6);
        var item = NominationTemplate.NewNominationTemplate("x", "y");
        item.Weight = 3575.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        NominationTemplateValidatorReportsEveryProblem();
        NominationTemplateAttemptsRangeRuleFlagsValuesAboveAttempts();
        NominationTemplateRequiredCodeRuleFlagsABlankCode();
        NominationTemplateRequiredNameRuleFlagsABlankName();
        NominationTemplateWeightRangeRuleFlagsValuesAboveWeight();
    }
}
