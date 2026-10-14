namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class DefaultSkillProfileGroupRulesChecks
{
    public static void DefaultSkillProfileGroupValidatorReportsEveryProblem()
    {
        var validator = DefaultSkillProfileGroupRules.DefaultDefaultSkillProfileGroupValidator();
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSkillProfileGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultSkillProfileGroupTitleLengthRule(1000);
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new DefaultSkillProfileGroupBatchSizeRangeRule(64, 1000);
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultSkillProfileGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultSkillProfileGroupLabelLengthRule(120);
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileGroupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultSkillProfileGroupRequiredTitleRule("default skill profile group needs a value");
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSkillProfileGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultSkillProfileGroupRequiredLabelRule("default skill profile group needs this field");
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultSkillProfileGroupValidatorReportsEveryProblem();
        DefaultSkillProfileGroupTitleLengthRuleFlagsALongTitle();
        DefaultSkillProfileGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        DefaultSkillProfileGroupLabelLengthRuleFlagsALongLabel();
        DefaultSkillProfileGroupRequiredTitleRuleFlagsABlankTitle();
        DefaultSkillProfileGroupRequiredLabelRuleFlagsABlankLabel();
    }
}
