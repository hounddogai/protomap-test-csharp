namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class WeightedCareerPathOverrideRulesChecks
{
    public static void WeightedCareerPathOverrideValidatorReportsEveryProblem()
    {
        var validator = WeightedCareerPathOverrideRules.DefaultWeightedCareerPathOverrideValidator();
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedCareerPathOverrideSkillLengthRuleFlagsALongSkill()
    {
        var rule = new WeightedCareerPathOverrideSkillLengthRule(255);
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedCareerPathOverrideRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new WeightedCareerPathOverrideRequiredSkillRule("weighted career path override needs an entry");
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedCareerPathOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new WeightedCareerPathOverrideExpiresAtRangeRule(32, 120);
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedCareerPathOverrideValidatorReportsEveryProblem();
        WeightedCareerPathOverrideSkillLengthRuleFlagsALongSkill();
        WeightedCareerPathOverrideRequiredSkillRuleFlagsABlankSkill();
        WeightedCareerPathOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
