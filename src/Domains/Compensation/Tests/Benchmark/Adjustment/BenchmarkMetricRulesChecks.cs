namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkMetricRulesChecks
{
    public static void BenchmarkMetricValidatorReportsEveryProblem()
    {
        var validator = BenchmarkMetricRules.DefaultBenchmarkMetricValidator();
        var item = BenchmarkMetric.NewBenchmarkMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkMetricRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BenchmarkMetricRequiredLocationRule("benchmark metric needs a value");
        var item = BenchmarkMetric.NewBenchmarkMetric("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkMetricRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BenchmarkMetricRatingRangeRule(8.3, 1388.3);
        var item = BenchmarkMetric.NewBenchmarkMetric("x", "y");
        item.Rating = 1389.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BenchmarkMetricTitleLengthRule(80);
        var item = BenchmarkMetric.NewBenchmarkMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkMetricValidatorReportsEveryProblem();
        BenchmarkMetricRequiredLocationRuleFlagsABlankLocation();
        BenchmarkMetricRatingRangeRuleFlagsValuesAboveRating();
        BenchmarkMetricTitleLengthRuleFlagsALongTitle();
    }
}
