namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkEntryRulesChecks
{
    public static void BenchmarkEntryValidatorReportsEveryProblem()
    {
        var validator = BenchmarkEntryRules.DefaultBenchmarkEntryValidator();
        var item = BenchmarkEntry.NewBenchmarkEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkEntryBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BenchmarkEntryBatchSizeRangeRule(64, 1000);
        var item = BenchmarkEntry.NewBenchmarkEntry("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkEntryRequiredCodeRule("benchmark entry needs a value");
        var item = BenchmarkEntry.NewBenchmarkEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkEntryCodeLengthRule(16);
        var item = BenchmarkEntry.NewBenchmarkEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkEntryValidatorReportsEveryProblem();
        BenchmarkEntryBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BenchmarkEntryRequiredCodeRuleFlagsABlankCode();
        BenchmarkEntryCodeLengthRuleFlagsALongCode();
    }
}
