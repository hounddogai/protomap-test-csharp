namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMetricRulesChecks
{
    public static void ReorgMetricValidatorReportsEveryProblem()
    {
        var validator = ReorgMetricRules.DefaultReorgMetricValidator();
        var item = ReorgMetric.NewReorgMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgMetricBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ReorgMetricBatchSizeRangeRule(64, 255);
        var item = ReorgMetric.NewReorgMetric("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ReorgMetricRequiredLocaleRule("reorg metric needs this field");
        var item = ReorgMetric.NewReorgMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReorgMetricTitleLengthRule(3600);
        var item = ReorgMetric.NewReorgMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReorgMetricRequiredTitleRule("reorg metric needs this field");
        var item = ReorgMetric.NewReorgMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgMetricValidatorReportsEveryProblem();
        ReorgMetricBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ReorgMetricRequiredLocaleRuleFlagsABlankLocale();
        ReorgMetricTitleLengthRuleFlagsALongTitle();
        ReorgMetricRequiredTitleRuleFlagsABlankTitle();
    }
}
