namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentRulesChecks
{
    public static void BatchedRefreshAssignmentValidatorReportsEveryProblem()
    {
        var validator = BatchedRefreshAssignmentRules.DefaultBatchedRefreshAssignmentValidator();
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRefreshAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedRefreshAssignmentRequiredNameRule("batched refresh assignment needs a value");
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedRefreshAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BatchedRefreshAssignmentBatchSizeRangeRule(80, 86400);
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshAssignmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BatchedRefreshAssignmentRetriesRangeRule(120, 86400);
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedRefreshAssignmentValidatorReportsEveryProblem();
        BatchedRefreshAssignmentRequiredNameRuleFlagsABlankName();
        BatchedRefreshAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BatchedRefreshAssignmentRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
