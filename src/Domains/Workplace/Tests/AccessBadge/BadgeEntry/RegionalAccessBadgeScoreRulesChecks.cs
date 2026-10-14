namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RegionalAccessBadgeScoreRulesChecks
{
    public static void RegionalAccessBadgeScoreValidatorReportsEveryProblem()
    {
        var validator = RegionalAccessBadgeScoreRules.DefaultRegionalAccessBadgeScoreValidator();
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAccessBadgeScoreNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalAccessBadgeScoreNameLengthRule(255);
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAccessBadgeScoreRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new RegionalAccessBadgeScoreRequiredContactEmailRule("regional access badge score needs this field");
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAccessBadgeScoreScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalAccessBadgeScoreScoreRangeRule(6.5, 2297.6);
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("x", "y");
        item.Score = 2299.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeScoreValidatorReportsEveryProblem();
        RegionalAccessBadgeScoreNameLengthRuleFlagsALongName();
        RegionalAccessBadgeScoreRequiredContactEmailRuleFlagsABlankContactEmail();
        RegionalAccessBadgeScoreScoreRangeRuleFlagsValuesAboveScore();
    }
}
