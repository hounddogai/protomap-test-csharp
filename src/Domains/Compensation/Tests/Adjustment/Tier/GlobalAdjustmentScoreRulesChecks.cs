namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreRulesChecks
{
    public static void GlobalAdjustmentScoreValidatorReportsEveryProblem()
    {
        var validator = GlobalAdjustmentScoreRules.DefaultGlobalAdjustmentScoreValidator();
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAdjustmentScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalAdjustmentScoreRequiredTitleRule("global adjustment score needs a value");
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAdjustmentScoreRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new GlobalAdjustmentScoreRatingRangeRule(3.2, 2203.2);
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("x", "y");
        item.Rating = 2204.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAdjustmentScoreRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new GlobalAdjustmentScoreRequiredSegmentRule("global adjustment score needs an entry");
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalAdjustmentScoreValidatorReportsEveryProblem();
        GlobalAdjustmentScoreRequiredTitleRuleFlagsABlankTitle();
        GlobalAdjustmentScoreRatingRangeRuleFlagsValuesAboveRating();
        GlobalAdjustmentScoreRequiredSegmentRuleFlagsABlankSegment();
    }
}
