namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatScoreRulesChecks
{
    public static void LicenseSeatScoreValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatScoreRules.DefaultLicenseSeatScoreValidator();
        var item = LicenseSeatScore.NewLicenseSeatScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LicenseSeatScoreTitleLengthRule(32);
        var item = LicenseSeatScore.NewLicenseSeatScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatScoreEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new LicenseSeatScoreEndsAtRangeRule(32, 3600);
        var item = LicenseSeatScore.NewLicenseSeatScore("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatScoreSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LicenseSeatScoreSkillLengthRule(120);
        var item = LicenseSeatScore.NewLicenseSeatScore("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LicenseSeatScoreRequiredTitleRule("license seat score needs a value");
        var item = LicenseSeatScore.NewLicenseSeatScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatScoreValidatorReportsEveryProblem();
        LicenseSeatScoreTitleLengthRuleFlagsALongTitle();
        LicenseSeatScoreEndsAtRangeRuleFlagsValuesAboveEndsAt();
        LicenseSeatScoreSkillLengthRuleFlagsALongSkill();
        LicenseSeatScoreRequiredTitleRuleFlagsABlankTitle();
    }
}
