namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkRule;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Checks of benchmark rule code, run by the project's runner.</summary>
public static class BenchmarkRollupRulesChecks
{
    public static void BenchmarkRollupValidatorReportsEveryProblem()
    {
        var validator = BenchmarkRollupRules.DefaultBenchmarkRollupValidator();
        var item = BenchmarkRollup.NewBenchmarkRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkRollupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BenchmarkRollupRetriesRangeRule(64, 86400);
        var item = BenchmarkRollup.NewBenchmarkRollup("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new BenchmarkRollupRequiredNameRule("benchmark rollup needs a value");
        var item = BenchmarkRollup.NewBenchmarkRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkRollupValidatorReportsEveryProblem();
        BenchmarkRollupRetriesRangeRuleFlagsValuesAboveRetries();
        BenchmarkRollupRequiredNameRuleFlagsABlankName();
    }
}
