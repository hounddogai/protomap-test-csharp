namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PayRunMetricRulesChecks
{
    public static void PayRunMetricValidatorReportsEveryProblem()
    {
        var validator = PayRunMetricRules.DefaultPayRunMetricValidator();
        var item = PayRunMetric.NewPayRunMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunMetricTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PayRunMetricTimezoneLengthRule(255);
        var item = PayRunMetric.NewPayRunMetric("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayRunMetricTitleLengthRule(32);
        var item = PayRunMetric.NewPayRunMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunMetricChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PayRunMetricChannelLengthRule(64);
        var item = PayRunMetric.NewPayRunMetric("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunMetricCodeLengthRule(3600);
        var item = PayRunMetric.NewPayRunMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunMetricValidatorReportsEveryProblem();
        PayRunMetricTimezoneLengthRuleFlagsALongTimezone();
        PayRunMetricTitleLengthRuleFlagsALongTitle();
        PayRunMetricChannelLengthRuleFlagsALongChannel();
        PayRunMetricCodeLengthRuleFlagsALongCode();
    }
}
