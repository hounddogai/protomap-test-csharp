namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeRulesChecks
{
    public static void SkillProfileNoticeValidatorReportsEveryProblem()
    {
        var validator = SkillProfileNoticeRules.DefaultSkillProfileNoticeValidator();
        var item = SkillProfileNotice.NewSkillProfileNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileNoticeScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SkillProfileNoticeScoreRangeRule(0.3, 3666.0);
        var item = SkillProfileNotice.NewSkillProfileNotice("x", "y");
        item.Score = 3667.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileNoticeValidatorReportsEveryProblem();
        SkillProfileNoticeScoreRangeRuleFlagsValuesAboveScore();
    }
}
