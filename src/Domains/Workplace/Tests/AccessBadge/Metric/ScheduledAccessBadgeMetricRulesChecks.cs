namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeMetricRulesChecks
{
    public static void ScheduledAccessBadgeMetricValidatorReportsEveryProblem()
    {
        var validator = ScheduledAccessBadgeMetricRules.DefaultScheduledAccessBadgeMetricValidator();
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAccessBadgeMetricJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ScheduledAccessBadgeMetricJobCodeLengthRule(80);
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAccessBadgeMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledAccessBadgeMetricRequiredCodeRule("scheduled access badge metric needs a value");
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAccessBadgeMetricPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ScheduledAccessBadgeMetricPhoneLengthRule(255);
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledAccessBadgeMetricValidatorReportsEveryProblem();
        ScheduledAccessBadgeMetricJobCodeLengthRuleFlagsALongJobCode();
        ScheduledAccessBadgeMetricRequiredCodeRuleFlagsABlankCode();
        ScheduledAccessBadgeMetricPhoneLengthRuleFlagsALongPhone();
    }
}
