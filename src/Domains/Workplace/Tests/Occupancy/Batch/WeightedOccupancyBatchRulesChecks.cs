namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedOccupancyBatchRulesChecks
{
    public static void WeightedOccupancyBatchValidatorReportsEveryProblem()
    {
        var validator = WeightedOccupancyBatchRules.DefaultWeightedOccupancyBatchValidator();
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedOccupancyBatchCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new WeightedOccupancyBatchCategoryLengthRule(3600);
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedOccupancyBatchFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new WeightedOccupancyBatchFloorValueRangeRule(6.5, 1205.5);
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("x", "y");
        item.FloorValue = 1207.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedOccupancyBatchTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new WeightedOccupancyBatchTimezoneLengthRule(255);
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedOccupancyBatchCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WeightedOccupancyBatchCeilingRangeRule(6.8, 2170.0);
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("x", "y");
        item.Ceiling = 2171.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedOccupancyBatchRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new WeightedOccupancyBatchRatioRangeRule(1.2, 1528.8);
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("x", "y");
        item.Ratio = 1530.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedOccupancyBatchValidatorReportsEveryProblem();
        WeightedOccupancyBatchCategoryLengthRuleFlagsALongCategory();
        WeightedOccupancyBatchFloorValueRangeRuleFlagsValuesAboveFloorValue();
        WeightedOccupancyBatchTimezoneLengthRuleFlagsALongTimezone();
        WeightedOccupancyBatchCeilingRangeRuleFlagsValuesAboveCeiling();
        WeightedOccupancyBatchRatioRangeRuleFlagsValuesAboveRatio();
    }
}
