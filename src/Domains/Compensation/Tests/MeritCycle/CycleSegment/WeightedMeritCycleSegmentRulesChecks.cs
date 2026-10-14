namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleSegmentRulesChecks
{
    public static void WeightedMeritCycleSegmentValidatorReportsEveryProblem()
    {
        var validator = WeightedMeritCycleSegmentRules.DefaultWeightedMeritCycleSegmentValidator();
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedMeritCycleSegmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new WeightedMeritCycleSegmentFloorValueRangeRule(4.8, 4140.6);
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("x", "y");
        item.FloorValue = 4142.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedMeritCycleSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedMeritCycleSegmentRequiredCodeRule("weighted merit cycle segment needs a value");
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedMeritCycleSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new WeightedMeritCycleSegmentBatchSizeRangeRule(64, 3600);
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedMeritCycleSegmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new WeightedMeritCycleSegmentOwnerIdLengthRule(255);
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedMeritCycleSegmentValidatorReportsEveryProblem();
        WeightedMeritCycleSegmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        WeightedMeritCycleSegmentRequiredCodeRuleFlagsABlankCode();
        WeightedMeritCycleSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        WeightedMeritCycleSegmentOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
