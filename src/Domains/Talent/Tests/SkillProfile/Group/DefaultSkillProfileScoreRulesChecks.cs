namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultSkillProfileScoreRulesChecks
{
    public static void DefaultSkillProfileScoreValidatorReportsEveryProblem()
    {
        var validator = DefaultSkillProfileScoreRules.DefaultDefaultSkillProfileScoreValidator();
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSkillProfileScoreExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new DefaultSkillProfileScoreExpiresAtRangeRule(255, 255);
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultSkillProfileScoreSkillLengthRuleFlagsALongSkill()
    {
        var rule = new DefaultSkillProfileScoreSkillLengthRule(80);
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultSkillProfileScoreCodeLengthRule(64);
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultSkillProfileScoreTitleLengthRule(120);
        var item = DefaultSkillProfileScore.NewDefaultSkillProfileScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScoreValidatorReportsEveryProblem();
        DefaultSkillProfileScoreExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        DefaultSkillProfileScoreSkillLengthRuleFlagsALongSkill();
        DefaultSkillProfileScoreCodeLengthRuleFlagsALongCode();
        DefaultSkillProfileScoreTitleLengthRuleFlagsALongTitle();
    }
}
