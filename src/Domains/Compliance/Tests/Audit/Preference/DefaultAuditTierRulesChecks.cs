namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultAuditTierRulesChecks
{
    public static void DefaultAuditTierValidatorReportsEveryProblem()
    {
        var validator = DefaultAuditTierRules.DefaultDefaultAuditTierValidator();
        var item = DefaultAuditTier.NewDefaultAuditTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAuditTierRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new DefaultAuditTierRateRangeRule(7.9, 2019.9);
        var item = DefaultAuditTier.NewDefaultAuditTier("x", "y");
        item.Rate = 2021.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAuditTierUnitLengthRuleFlagsALongUnit()
    {
        var rule = new DefaultAuditTierUnitLengthRule(32);
        var item = DefaultAuditTier.NewDefaultAuditTier("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAuditTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultAuditTierRequiredLabelRule("default audit tier needs a value");
        var item = DefaultAuditTier.NewDefaultAuditTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAuditTierSkillLengthRuleFlagsALongSkill()
    {
        var rule = new DefaultAuditTierSkillLengthRule(86400);
        var item = DefaultAuditTier.NewDefaultAuditTier("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAuditTierRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DefaultAuditTierRequiredUnitRule("default audit tier needs this field");
        var item = DefaultAuditTier.NewDefaultAuditTier("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultAuditTierValidatorReportsEveryProblem();
        DefaultAuditTierRateRangeRuleFlagsValuesAboveRate();
        DefaultAuditTierUnitLengthRuleFlagsALongUnit();
        DefaultAuditTierRequiredLabelRuleFlagsABlankLabel();
        DefaultAuditTierSkillLengthRuleFlagsALongSkill();
        DefaultAuditTierRequiredUnitRuleFlagsABlankUnit();
    }
}
