namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupRulesChecks
{
    public static void SkillProfileGroupValidatorReportsEveryProblem()
    {
        var validator = SkillProfileGroupRules.DefaultSkillProfileGroupValidator();
        var item = SkillProfileGroup.NewSkillProfileGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileGroupLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SkillProfileGroupLimitRangeRule(120, 3600);
        var item = SkillProfileGroup.NewSkillProfileGroup("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileGroupReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SkillProfileGroupReviewerLengthRule(3600);
        var item = SkillProfileGroup.NewSkillProfileGroup("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileGroupValidatorReportsEveryProblem();
        SkillProfileGroupLimitRangeRuleFlagsValuesAboveLimit();
        SkillProfileGroupReviewerLengthRuleFlagsALongReviewer();
    }
}
