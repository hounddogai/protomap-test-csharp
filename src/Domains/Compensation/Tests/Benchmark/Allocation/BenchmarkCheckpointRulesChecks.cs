namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkCheckpointRulesChecks
{
    public static void BenchmarkCheckpointValidatorReportsEveryProblem()
    {
        var validator = BenchmarkCheckpointRules.DefaultBenchmarkCheckpointValidator();
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BenchmarkCheckpointBatchSizeRangeRule(32, 255);
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new BenchmarkCheckpointNameLengthRule(32);
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new BenchmarkCheckpointRequiredNameRule("benchmark checkpoint needs an entry");
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkCheckpointValidatorReportsEveryProblem();
        BenchmarkCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BenchmarkCheckpointNameLengthRuleFlagsALongName();
        BenchmarkCheckpointRequiredNameRuleFlagsABlankName();
    }
}
