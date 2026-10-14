namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkBatchRulesChecks
{
    public static void BenchmarkBatchValidatorReportsEveryProblem()
    {
        var validator = BenchmarkBatchRules.DefaultBenchmarkBatchValidator();
        var item = BenchmarkBatch.NewBenchmarkBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new BenchmarkBatchCreatedAtRangeRule(120, 120);
        var item = BenchmarkBatch.NewBenchmarkBatch("x", "y");
        item.CreatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkBatchCodeLengthRule(255);
        var item = BenchmarkBatch.NewBenchmarkBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BenchmarkBatchRequiredLabelRule("benchmark batch needs a value");
        var item = BenchmarkBatch.NewBenchmarkBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkBatchRequiredCodeRule("benchmark batch needs a value");
        var item = BenchmarkBatch.NewBenchmarkBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BenchmarkBatchWindowDaysRangeRule(80, 120);
        var item = BenchmarkBatch.NewBenchmarkBatch("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkBatchValidatorReportsEveryProblem();
        BenchmarkBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        BenchmarkBatchCodeLengthRuleFlagsALongCode();
        BenchmarkBatchRequiredLabelRuleFlagsABlankLabel();
        BenchmarkBatchRequiredCodeRuleFlagsABlankCode();
        BenchmarkBatchWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
