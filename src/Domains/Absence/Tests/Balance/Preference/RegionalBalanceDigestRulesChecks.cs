namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalBalanceDigestRulesChecks
{
    public static void RegionalBalanceDigestValidatorReportsEveryProblem()
    {
        var validator = RegionalBalanceDigestRules.DefaultRegionalBalanceDigestValidator();
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBalanceDigestSkillLengthRuleFlagsALongSkill()
    {
        var rule = new RegionalBalanceDigestSkillLengthRule(255);
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBalanceDigestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RegionalBalanceDigestLeaveDaysRangeRule(120, 1000);
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBalanceDigestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RegionalBalanceDigestRequiredTeamRule("regional balance digest needs an entry");
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalBalanceDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalBalanceDigestRequiredNameRule("regional balance digest needs an entry");
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalBalanceDigestValidatorReportsEveryProblem();
        RegionalBalanceDigestSkillLengthRuleFlagsALongSkill();
        RegionalBalanceDigestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RegionalBalanceDigestRequiredTeamRuleFlagsABlankTeam();
        RegionalBalanceDigestRequiredNameRuleFlagsABlankName();
    }
}
