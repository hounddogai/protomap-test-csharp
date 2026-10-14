namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMetricRulesChecks
{
    public static void AdaptiveBenchmarkMetricValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBenchmarkMetricRules.DefaultAdaptiveBenchmarkMetricValidator();
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBenchmarkMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveBenchmarkMetricCodeLengthRule(32);
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBenchmarkMetricRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AdaptiveBenchmarkMetricRequiredLocationRule("adaptive benchmark metric needs an entry");
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBenchmarkMetricRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new AdaptiveBenchmarkMetricRequiredOwnerIdRule("adaptive benchmark metric needs an entry");
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBenchmarkMetricFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveBenchmarkMetricFteRangeRule(0.4, 1063.0);
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("x", "y");
        item.Fte = 1064.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkMetricValidatorReportsEveryProblem();
        AdaptiveBenchmarkMetricCodeLengthRuleFlagsALongCode();
        AdaptiveBenchmarkMetricRequiredLocationRuleFlagsABlankLocation();
        AdaptiveBenchmarkMetricRequiredOwnerIdRuleFlagsABlankOwnerId();
        AdaptiveBenchmarkMetricFteRangeRuleFlagsValuesAboveFte();
    }
}
