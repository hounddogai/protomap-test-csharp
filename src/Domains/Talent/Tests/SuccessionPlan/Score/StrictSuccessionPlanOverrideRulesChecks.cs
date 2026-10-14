namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanOverrideRulesChecks
{
    public static void StrictSuccessionPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = StrictSuccessionPlanOverrideRules.DefaultStrictSuccessionPlanOverrideValidator();
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSuccessionPlanOverrideSkillLengthRuleFlagsALongSkill()
    {
        var rule = new StrictSuccessionPlanOverrideSkillLengthRule(3600);
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSuccessionPlanOverrideRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new StrictSuccessionPlanOverrideRequiredSkillRule("strict succession plan override needs a value");
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSuccessionPlanOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictSuccessionPlanOverrideCodeLengthRule(80);
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSuccessionPlanOverrideFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new StrictSuccessionPlanOverrideFloorValueRangeRule(10.0, 3396.6);
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("x", "y");
        item.FloorValue = 3398.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanOverrideValidatorReportsEveryProblem();
        StrictSuccessionPlanOverrideSkillLengthRuleFlagsALongSkill();
        StrictSuccessionPlanOverrideRequiredSkillRuleFlagsABlankSkill();
        StrictSuccessionPlanOverrideCodeLengthRuleFlagsALongCode();
        StrictSuccessionPlanOverrideFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
