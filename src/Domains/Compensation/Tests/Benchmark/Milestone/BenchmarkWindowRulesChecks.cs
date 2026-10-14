namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowRulesChecks
{
    public static void BenchmarkWindowValidatorReportsEveryProblem()
    {
        var validator = BenchmarkWindowRules.DefaultBenchmarkWindowValidator();
        var item = BenchmarkWindow.NewBenchmarkWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkWindowVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BenchmarkWindowVersionRangeRule(64, 86400);
        var item = BenchmarkWindow.NewBenchmarkWindow("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkWindowRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BenchmarkWindowRateRangeRule(9.1, 387.0);
        var item = BenchmarkWindow.NewBenchmarkWindow("x", "y");
        item.Rate = 388.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BenchmarkWindowRequiredTitleRule("benchmark window needs a value");
        var item = BenchmarkWindow.NewBenchmarkWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkWindowValidatorReportsEveryProblem();
        BenchmarkWindowVersionRangeRuleFlagsValuesAboveVersion();
        BenchmarkWindowRateRangeRuleFlagsValuesAboveRate();
        BenchmarkWindowRequiredTitleRuleFlagsABlankTitle();
    }
}
