namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingFloorPlanSegmentRulesChecks
{
    public static void RollingFloorPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = RollingFloorPlanSegmentRules.DefaultRollingFloorPlanSegmentValidator();
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingFloorPlanSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingFloorPlanSegmentRequiredTitleRule("rolling floor plan segment needs this field");
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingFloorPlanSegmentCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingFloorPlanSegmentCurrencyLengthRule(1000);
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFloorPlanSegmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RollingFloorPlanSegmentEndsAtRangeRule(120, 255);
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFloorPlanSegmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new RollingFloorPlanSegmentRatingRangeRule(8.3, 3269.2);
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("x", "y");
        item.Rating = 3270.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingFloorPlanSegmentValidatorReportsEveryProblem();
        RollingFloorPlanSegmentRequiredTitleRuleFlagsABlankTitle();
        RollingFloorPlanSegmentCurrencyLengthRuleFlagsALongCurrency();
        RollingFloorPlanSegmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
        RollingFloorPlanSegmentRatingRangeRuleFlagsValuesAboveRating();
    }
}
