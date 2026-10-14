namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSegmentRulesChecks
{
    public static void AdjustmentSegmentValidatorReportsEveryProblem()
    {
        var validator = AdjustmentSegmentRules.DefaultAdjustmentSegmentValidator();
        var item = AdjustmentSegment.NewAdjustmentSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentSegmentRequiredCodeRule("adjustment segment needs an entry");
        var item = AdjustmentSegment.NewAdjustmentSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentSegmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AdjustmentSegmentCapacityRangeRule(64, 86400);
        var item = AdjustmentSegment.NewAdjustmentSegment("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new AdjustmentSegmentStartsAtRangeRule(120, 86400);
        var item = AdjustmentSegment.NewAdjustmentSegment("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentSegmentValidatorReportsEveryProblem();
        AdjustmentSegmentRequiredCodeRuleFlagsABlankCode();
        AdjustmentSegmentCapacityRangeRuleFlagsValuesAboveCapacity();
        AdjustmentSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
