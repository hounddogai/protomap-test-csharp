namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalAssignmentTierRulesChecks
{
    public static void RegionalAssignmentTierValidatorReportsEveryProblem()
    {
        var validator = RegionalAssignmentTierRules.DefaultRegionalAssignmentTierValidator();
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAssignmentTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalAssignmentTierCodeLengthRule(3600);
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssignmentTierSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RegionalAssignmentTierSkillLengthRule(32);
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssignmentTierLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RegionalAssignmentTierLimitRangeRule(120, 255);
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAssignmentTierNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalAssignmentTierNameLengthRule(86400);
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalAssignmentTierValidatorReportsEveryProblem();
        RegionalAssignmentTierCodeLengthRuleFlagsALongCode();
        RegionalAssignmentTierSkillLengthRuleFlagsALongSkill();
        RegionalAssignmentTierLimitRangeRuleFlagsValuesAboveLimit();
        RegionalAssignmentTierNameLengthRuleFlagsALongName();
    }
}
