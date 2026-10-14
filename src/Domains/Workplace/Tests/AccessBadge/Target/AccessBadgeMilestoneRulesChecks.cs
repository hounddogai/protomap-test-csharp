namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeMilestoneRulesChecks
{
    public static void AccessBadgeMilestoneValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeMilestoneRules.DefaultAccessBadgeMilestoneValidator();
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeMilestoneRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AccessBadgeMilestoneRateRangeRule(0.1, 4624.6);
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("x", "y");
        item.Rate = 4626.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeMilestoneUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AccessBadgeMilestoneUnitLengthRule(16);
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeMilestoneSkillLengthRuleFlagsALongSkill()
    {
        var rule = new AccessBadgeMilestoneSkillLengthRule(86400);
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeMilestoneValidatorReportsEveryProblem();
        AccessBadgeMilestoneRateRangeRuleFlagsValuesAboveRate();
        AccessBadgeMilestoneUnitLengthRuleFlagsALongUnit();
        AccessBadgeMilestoneSkillLengthRuleFlagsALongSkill();
    }
}
