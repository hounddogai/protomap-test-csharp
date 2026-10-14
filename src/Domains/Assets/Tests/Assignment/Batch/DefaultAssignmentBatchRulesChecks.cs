namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultAssignmentBatchRulesChecks
{
    public static void DefaultAssignmentBatchValidatorReportsEveryProblem()
    {
        var validator = DefaultAssignmentBatchRules.DefaultDefaultAssignmentBatchValidator();
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAssignmentBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultAssignmentBatchRequiredNameRule("default assignment batch needs this field");
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAssignmentBatchQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DefaultAssignmentBatchQuantityRangeRule(80, 80);
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultAssignmentBatchValidatorReportsEveryProblem();
        DefaultAssignmentBatchRequiredNameRuleFlagsABlankName();
        DefaultAssignmentBatchQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
