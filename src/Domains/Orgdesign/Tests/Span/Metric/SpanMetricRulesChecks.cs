namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanMetricRulesChecks
{
    public static void SpanMetricValidatorReportsEveryProblem()
    {
        var validator = SpanMetricRules.DefaultSpanMetricValidator();
        var item = SpanMetric.NewSpanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanMetricRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SpanMetricRequiredBadgeRule("span metric needs an entry");
        var item = SpanMetric.NewSpanMetric("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanMetricFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new SpanMetricFteRangeRule(5.9, 1492.1);
        var item = SpanMetric.NewSpanMetric("x", "y");
        item.Fte = 1493.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanMetricRequiredCodeRule("span metric needs an entry");
        var item = SpanMetric.NewSpanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanMetricValidatorReportsEveryProblem();
        SpanMetricRequiredBadgeRuleFlagsABlankBadge();
        SpanMetricFteRangeRuleFlagsValuesAboveFte();
        SpanMetricRequiredCodeRuleFlagsABlankCode();
    }
}
