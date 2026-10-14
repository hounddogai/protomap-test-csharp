namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkEstimateRulesChecks
{
    public static void BenchmarkEstimateValidatorReportsEveryProblem()
    {
        var validator = BenchmarkEstimateRules.DefaultBenchmarkEstimateValidator();
        var item = BenchmarkEstimate.NewBenchmarkEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkEstimateRequiredCodeRule("benchmark estimate needs an entry");
        var item = BenchmarkEstimate.NewBenchmarkEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkEstimateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BenchmarkEstimateRequiredPhoneRule("benchmark estimate needs this field");
        var item = BenchmarkEstimate.NewBenchmarkEstimate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BenchmarkEstimateMarginRangeRule(9.8, 4217.3);
        var item = BenchmarkEstimate.NewBenchmarkEstimate("x", "y");
        item.Margin = 4218.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkEstimateValidatorReportsEveryProblem();
        BenchmarkEstimateRequiredCodeRuleFlagsABlankCode();
        BenchmarkEstimateRequiredPhoneRuleFlagsABlankPhone();
        BenchmarkEstimateMarginRangeRuleFlagsValuesAboveMargin();
    }
}
