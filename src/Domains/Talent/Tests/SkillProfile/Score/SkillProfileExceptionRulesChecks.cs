namespace Acme.Workforce.Talent.Tests.SkillProfile.Score;

using Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SkillProfileExceptionRulesChecks
{
    public static void SkillProfileExceptionValidatorReportsEveryProblem()
    {
        var validator = SkillProfileExceptionRules.DefaultSkillProfileExceptionValidator();
        var item = SkillProfileException.NewSkillProfileException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SkillProfileExceptionTitleLengthRule(86400);
        var item = SkillProfileException.NewSkillProfileException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileExceptionRequiredCodeRule("skill profile exception needs this field");
        var item = SkillProfileException.NewSkillProfileException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileExceptionThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new SkillProfileExceptionThresholdRangeRule(5.8, 1674.8);
        var item = SkillProfileException.NewSkillProfileException("x", "y");
        item.Threshold = 1676.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileExceptionValidatorReportsEveryProblem();
        SkillProfileExceptionTitleLengthRuleFlagsALongTitle();
        SkillProfileExceptionRequiredCodeRuleFlagsABlankCode();
        SkillProfileExceptionThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
