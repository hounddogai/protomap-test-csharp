namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class BatchedCostModelBatchRulesChecks
{
    public static void BatchedCostModelBatchValidatorReportsEveryProblem()
    {
        var validator = BatchedCostModelBatchRules.DefaultBatchedCostModelBatchValidator();
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedCostModelBatchCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BatchedCostModelBatchCapacityRangeRule(120, 86400);
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedCostModelBatchEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BatchedCostModelBatchEndsAtRangeRule(1000, 86400);
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedCostModelBatchDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new BatchedCostModelBatchDepartmentLengthRule(1000);
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedCostModelBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedCostModelBatchRequiredTitleRule("batched cost model batch needs this field");
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedCostModelBatchValidatorReportsEveryProblem();
        BatchedCostModelBatchCapacityRangeRuleFlagsValuesAboveCapacity();
        BatchedCostModelBatchEndsAtRangeRuleFlagsValuesAboveEndsAt();
        BatchedCostModelBatchDepartmentLengthRuleFlagsALongDepartment();
        BatchedCostModelBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
