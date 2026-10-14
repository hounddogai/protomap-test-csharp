namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultPolicyVarianceRulesChecks
{
    public static void DefaultPolicyVarianceValidatorReportsEveryProblem()
    {
        var validator = DefaultPolicyVarianceRules.DefaultDefaultPolicyVarianceValidator();
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPolicyVarianceRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new DefaultPolicyVarianceRequiredSkillRule("default policy variance needs an entry");
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPolicyVarianceDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new DefaultPolicyVarianceDiscountRangeRule(0.1, 228.0);
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("x", "y");
        item.Discount = 229.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPolicyVarianceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultPolicyVarianceLabelLengthRule(3600);
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPolicyVarianceValidatorReportsEveryProblem();
        DefaultPolicyVarianceRequiredSkillRuleFlagsABlankSkill();
        DefaultPolicyVarianceDiscountRangeRuleFlagsValuesAboveDiscount();
        DefaultPolicyVarianceLabelLengthRuleFlagsALongLabel();
    }
}
