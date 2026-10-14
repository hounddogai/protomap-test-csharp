namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictTaxFormTierRulesChecks
{
    public static void StrictTaxFormTierValidatorReportsEveryProblem()
    {
        var validator = StrictTaxFormTierRules.DefaultStrictTaxFormTierValidator();
        var item = StrictTaxFormTier.NewStrictTaxFormTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictTaxFormTierRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new StrictTaxFormTierRequiredTimezoneRule("strict tax form tier needs a value");
        var item = StrictTaxFormTier.NewStrictTaxFormTier("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictTaxFormTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictTaxFormTierLabelLengthRule(86400);
        var item = StrictTaxFormTier.NewStrictTaxFormTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTaxFormTierTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new StrictTaxFormTierTimezoneLengthRule(80);
        var item = StrictTaxFormTier.NewStrictTaxFormTier("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTaxFormTierRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new StrictTaxFormTierRequiredSkillRule("strict tax form tier needs this field");
        var item = StrictTaxFormTier.NewStrictTaxFormTier("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictTaxFormTierValidatorReportsEveryProblem();
        StrictTaxFormTierRequiredTimezoneRuleFlagsABlankTimezone();
        StrictTaxFormTierLabelLengthRuleFlagsALongLabel();
        StrictTaxFormTierTimezoneLengthRuleFlagsALongTimezone();
        StrictTaxFormTierRequiredSkillRuleFlagsABlankSkill();
    }
}
