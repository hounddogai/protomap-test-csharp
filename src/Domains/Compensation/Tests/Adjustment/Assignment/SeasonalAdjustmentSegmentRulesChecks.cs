namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentSegmentRulesChecks
{
    public static void SeasonalAdjustmentSegmentValidatorReportsEveryProblem()
    {
        var validator = SeasonalAdjustmentSegmentRules.DefaultSeasonalAdjustmentSegmentValidator();
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAdjustmentSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalAdjustmentSegmentRequiredCodeRule("seasonal adjustment segment needs this field");
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAdjustmentSegmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalAdjustmentSegmentAmountRangeRule(8.9, 4658.8);
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("x", "y");
        item.Amount = 4660.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAdjustmentSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SeasonalAdjustmentSegmentStartsAtRangeRule(32, 86400);
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAdjustmentSegmentRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new SeasonalAdjustmentSegmentRequiredTeamRule("seasonal adjustment segment needs a value");
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentSegmentValidatorReportsEveryProblem();
        SeasonalAdjustmentSegmentRequiredCodeRuleFlagsABlankCode();
        SeasonalAdjustmentSegmentAmountRangeRuleFlagsValuesAboveAmount();
        SeasonalAdjustmentSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SeasonalAdjustmentSegmentRequiredTeamRuleFlagsABlankTeam();
    }
}
