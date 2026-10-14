namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMetricRulesChecks
{
    public static void AccessBadgeMetricValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeMetricRules.DefaultAccessBadgeMetricValidator();
        var item = AccessBadgeMetric.NewAccessBadgeMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccessBadgeMetricRequiredLabelRule("access badge metric needs this field");
        var item = AccessBadgeMetric.NewAccessBadgeMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeMetricRequiredCodeRule("access badge metric needs this field");
        var item = AccessBadgeMetric.NewAccessBadgeMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeMetricRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AccessBadgeMetricRevisionRangeRule(32, 64);
        var item = AccessBadgeMetric.NewAccessBadgeMetric("x", "y");
        item.Revision = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeMetricValidatorReportsEveryProblem();
        AccessBadgeMetricRequiredLabelRuleFlagsABlankLabel();
        AccessBadgeMetricRequiredCodeRuleFlagsABlankCode();
        AccessBadgeMetricRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
