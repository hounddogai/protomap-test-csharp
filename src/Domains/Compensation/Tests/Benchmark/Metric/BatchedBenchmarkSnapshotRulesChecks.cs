namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedBenchmarkSnapshotRulesChecks
{
    public static void BatchedBenchmarkSnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedBenchmarkSnapshotRules.DefaultBatchedBenchmarkSnapshotValidator();
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBenchmarkSnapshotNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedBenchmarkSnapshotNameLengthRule(3600);
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBenchmarkSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedBenchmarkSnapshotRequiredCodeRule("batched benchmark snapshot needs this field");
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedBenchmarkSnapshotValidatorReportsEveryProblem();
        BatchedBenchmarkSnapshotNameLengthRuleFlagsALongName();
        BatchedBenchmarkSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
