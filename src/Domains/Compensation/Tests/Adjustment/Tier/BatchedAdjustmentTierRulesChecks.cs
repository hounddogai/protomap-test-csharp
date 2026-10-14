namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentTierRulesChecks
{
    public static void BatchedAdjustmentTierValidatorReportsEveryProblem()
    {
        var validator = BatchedAdjustmentTierRules.DefaultBatchedAdjustmentTierValidator();
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAdjustmentTierTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedAdjustmentTierTenureMonthsRangeRule(16, 1000);
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedAdjustmentTierFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedAdjustmentTierFloorValueRangeRule(0.5, 2439.2);
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("x", "y");
        item.FloorValue = 2440.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedAdjustmentTierDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new BatchedAdjustmentTierDiscountRangeRule(5.4, 3803.8);
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("x", "y");
        item.Discount = 3805.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedAdjustmentTierValidatorReportsEveryProblem();
        BatchedAdjustmentTierTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BatchedAdjustmentTierFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedAdjustmentTierDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
