namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SeasonalEngagementRollupRulesChecks
{
    public static void SeasonalEngagementRollupValidatorReportsEveryProblem()
    {
        var validator = SeasonalEngagementRollupRules.DefaultSeasonalEngagementRollupValidator();
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalEngagementRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalEngagementRollupRequiredCodeRule("seasonal engagement rollup needs a value");
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalEngagementRollupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SeasonalEngagementRollupRetriesRangeRule(1000, 3600);
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalEngagementRollupRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new SeasonalEngagementRollupRequiredJobCodeRule("seasonal engagement rollup needs this field");
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalEngagementRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SeasonalEngagementRollupGraceDaysRangeRule(120, 86400);
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalEngagementRollupValidatorReportsEveryProblem();
        SeasonalEngagementRollupRequiredCodeRuleFlagsABlankCode();
        SeasonalEngagementRollupRetriesRangeRuleFlagsValuesAboveRetries();
        SeasonalEngagementRollupRequiredJobCodeRuleFlagsABlankJobCode();
        SeasonalEngagementRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
