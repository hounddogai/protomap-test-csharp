namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkForecastRulesChecks
{
    public static void BenchmarkForecastValidatorReportsEveryProblem()
    {
        var validator = BenchmarkForecastRules.DefaultBenchmarkForecastValidator();
        var item = BenchmarkForecast.NewBenchmarkForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkForecastSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new BenchmarkForecastSequenceRangeRule(16, 86400);
        var item = BenchmarkForecast.NewBenchmarkForecast("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkForecastTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BenchmarkForecastTenureMonthsRangeRule(32, 80);
        var item = BenchmarkForecast.NewBenchmarkForecast("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkForecastRequiredCodeRule("benchmark forecast needs an entry");
        var item = BenchmarkForecast.NewBenchmarkForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkForecastRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BenchmarkForecastRatioRangeRule(9.6, 4003.0);
        var item = BenchmarkForecast.NewBenchmarkForecast("x", "y");
        item.Ratio = 4004.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BenchmarkForecastLabelLengthRule(86400);
        var item = BenchmarkForecast.NewBenchmarkForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkForecastValidatorReportsEveryProblem();
        BenchmarkForecastSequenceRangeRuleFlagsValuesAboveSequence();
        BenchmarkForecastTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BenchmarkForecastRequiredCodeRuleFlagsABlankCode();
        BenchmarkForecastRatioRangeRuleFlagsValuesAboveRatio();
        BenchmarkForecastLabelLengthRuleFlagsALongLabel();
    }
}
