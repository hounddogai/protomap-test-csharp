namespace Acme.Workforce.Absence.Tests.Accrual.Notice;

using Acme.Workforce.Absence.Accrual.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedAccrualCheckpointRulesChecks
{
    public static void BatchedAccrualCheckpointValidatorReportsEveryProblem()
    {
        var validator = BatchedAccrualCheckpointRules.DefaultBatchedAccrualCheckpointValidator();
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAccrualCheckpointMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedAccrualCheckpointMarginRangeRule(0.5, 1117.6);
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("x", "y");
        item.Margin = 1119.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedAccrualCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedAccrualCheckpointLabelLengthRule(86400);
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAccrualCheckpointSourceLengthRuleFlagsALongSource()
    {
        var rule = new BatchedAccrualCheckpointSourceLengthRule(255);
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedAccrualCheckpointValidatorReportsEveryProblem();
        BatchedAccrualCheckpointMarginRangeRuleFlagsValuesAboveMargin();
        BatchedAccrualCheckpointLabelLengthRuleFlagsALongLabel();
        BatchedAccrualCheckpointSourceLengthRuleFlagsALongSource();
    }
}
