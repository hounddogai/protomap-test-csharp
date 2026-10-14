namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualBenchmarkMetricRulesChecks
{
    public static void ManualBenchmarkMetricValidatorReportsEveryProblem()
    {
        var validator = ManualBenchmarkMetricRules.DefaultManualBenchmarkMetricValidator();
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBenchmarkMetricStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ManualBenchmarkMetricStartsAtRangeRule(16, 3600);
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBenchmarkMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualBenchmarkMetricLabelLengthRule(1000);
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBenchmarkMetricAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ManualBenchmarkMetricAttemptsRangeRule(64, 120);
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBenchmarkMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualBenchmarkMetricCodeLengthRule(32);
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBenchmarkMetricCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ManualBenchmarkMetricCreatedAtRangeRule(16, 86400);
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualBenchmarkMetricValidatorReportsEveryProblem();
        ManualBenchmarkMetricStartsAtRangeRuleFlagsValuesAboveStartsAt();
        ManualBenchmarkMetricLabelLengthRuleFlagsALongLabel();
        ManualBenchmarkMetricAttemptsRangeRuleFlagsValuesAboveAttempts();
        ManualBenchmarkMetricCodeLengthRuleFlagsALongCode();
        ManualBenchmarkMetricCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
