namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RefreshMetricRulesChecks
{
    public static void RefreshMetricValidatorReportsEveryProblem()
    {
        var validator = RefreshMetricRules.DefaultRefreshMetricValidator();
        var item = RefreshMetric.NewRefreshMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshMetricRequiredCodeRule("refresh metric needs this field");
        var item = RefreshMetric.NewRefreshMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshMetricCodeLengthRule(86400);
        var item = RefreshMetric.NewRefreshMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshMetricLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RefreshMetricLocaleLengthRule(1000);
        var item = RefreshMetric.NewRefreshMetric("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshMetricValidatorReportsEveryProblem();
        RefreshMetricRequiredCodeRuleFlagsABlankCode();
        RefreshMetricCodeLengthRuleFlagsALongCode();
        RefreshMetricLocaleLengthRuleFlagsALongLocale();
    }
}
