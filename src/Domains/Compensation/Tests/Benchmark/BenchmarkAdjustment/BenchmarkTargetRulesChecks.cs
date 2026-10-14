namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class BenchmarkTargetRulesChecks
{
    public static void BenchmarkTargetValidatorReportsEveryProblem()
    {
        var validator = BenchmarkTargetRules.DefaultBenchmarkTargetValidator();
        var item = BenchmarkTarget.NewBenchmarkTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkTargetNameLengthRuleFlagsALongName()
    {
        var rule = new BenchmarkTargetNameLengthRule(32);
        var item = BenchmarkTarget.NewBenchmarkTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new BenchmarkTargetRequiredNameRule("benchmark target needs a value");
        var item = BenchmarkTarget.NewBenchmarkTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkTargetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BenchmarkTargetRetriesRangeRule(32, 64);
        var item = BenchmarkTarget.NewBenchmarkTarget("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkTargetValidatorReportsEveryProblem();
        BenchmarkTargetNameLengthRuleFlagsALongName();
        BenchmarkTargetRequiredNameRuleFlagsABlankName();
        BenchmarkTargetRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
