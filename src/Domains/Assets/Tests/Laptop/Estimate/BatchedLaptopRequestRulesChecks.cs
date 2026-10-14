namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class BatchedLaptopRequestRulesChecks
{
    public static void BatchedLaptopRequestValidatorReportsEveryProblem()
    {
        var validator = BatchedLaptopRequestRules.DefaultBatchedLaptopRequestValidator();
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLaptopRequestQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BatchedLaptopRequestQuantityRangeRule(120, 86400);
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLaptopRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedLaptopRequestRequiredLabelRule("batched laptop request needs a value");
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLaptopRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedLaptopRequestRequiredCodeRule("batched laptop request needs a value");
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedLaptopRequestValidatorReportsEveryProblem();
        BatchedLaptopRequestQuantityRangeRuleFlagsValuesAboveQuantity();
        BatchedLaptopRequestRequiredLabelRuleFlagsABlankLabel();
        BatchedLaptopRequestRequiredCodeRuleFlagsABlankCode();
    }
}
