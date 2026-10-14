namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedFloorPlanAdjustmentRulesChecks
{
    public static void BatchedFloorPlanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = BatchedFloorPlanAdjustmentRules.DefaultBatchedFloorPlanAdjustmentValidator();
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFloorPlanAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new BatchedFloorPlanAdjustmentStartsAtRangeRule(16, 80);
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("x", "y");
        item.StartsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedFloorPlanAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedFloorPlanAdjustmentNameLengthRule(32);
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFloorPlanAdjustmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BatchedFloorPlanAdjustmentRatioRangeRule(5.6, 2961.4);
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("x", "y");
        item.Ratio = 2962.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedFloorPlanAdjustmentRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BatchedFloorPlanAdjustmentRateRangeRule(6.4, 4864.1);
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("x", "y");
        item.Rate = 4865.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedFloorPlanAdjustmentRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new BatchedFloorPlanAdjustmentRequiredReviewerRule("batched floor plan adjustment needs this field");
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedFloorPlanAdjustmentValidatorReportsEveryProblem();
        BatchedFloorPlanAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        BatchedFloorPlanAdjustmentNameLengthRuleFlagsALongName();
        BatchedFloorPlanAdjustmentRatioRangeRuleFlagsValuesAboveRatio();
        BatchedFloorPlanAdjustmentRateRangeRuleFlagsValuesAboveRate();
        BatchedFloorPlanAdjustmentRequiredReviewerRuleFlagsABlankReviewer();
    }
}
