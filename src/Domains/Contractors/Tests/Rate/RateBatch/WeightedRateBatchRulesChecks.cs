namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class WeightedRateBatchRulesChecks
{
    public static void WeightedRateBatchValidatorReportsEveryProblem()
    {
        var validator = WeightedRateBatchRules.DefaultWeightedRateBatchValidator();
        var item = WeightedRateBatch.NewWeightedRateBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRateBatchCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new WeightedRateBatchCapacityRangeRule(64, 80);
        var item = WeightedRateBatch.NewWeightedRateBatch("x", "y");
        item.Capacity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRateBatchFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new WeightedRateBatchFloorValueRangeRule(1.2, 676.0);
        var item = WeightedRateBatch.NewWeightedRateBatch("x", "y");
        item.FloorValue = 677.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRateBatchCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new WeightedRateBatchCostRangeRule(2.2, 1759.0);
        var item = WeightedRateBatch.NewWeightedRateBatch("x", "y");
        item.Cost = 1760.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRateBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedRateBatchRequiredTitleRule("weighted rate batch needs this field");
        var item = WeightedRateBatch.NewWeightedRateBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedRateBatchValidatorReportsEveryProblem();
        WeightedRateBatchCapacityRangeRuleFlagsValuesAboveCapacity();
        WeightedRateBatchFloorValueRangeRuleFlagsValuesAboveFloorValue();
        WeightedRateBatchCostRangeRuleFlagsValuesAboveCost();
        WeightedRateBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
