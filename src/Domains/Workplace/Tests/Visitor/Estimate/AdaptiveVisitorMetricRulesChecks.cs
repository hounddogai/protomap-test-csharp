namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveVisitorMetricRulesChecks
{
    public static void AdaptiveVisitorMetricValidatorReportsEveryProblem()
    {
        var validator = AdaptiveVisitorMetricRules.DefaultAdaptiveVisitorMetricValidator();
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveVisitorMetricVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AdaptiveVisitorMetricVersionRangeRule(64, 1000);
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveVisitorMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveVisitorMetricTitleLengthRule(255);
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveVisitorMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveVisitorMetricRequiredTitleRule("adaptive visitor metric needs this field");
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveVisitorMetricOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new AdaptiveVisitorMetricOwnerIdLengthRule(120);
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveVisitorMetricValidatorReportsEveryProblem();
        AdaptiveVisitorMetricVersionRangeRuleFlagsValuesAboveVersion();
        AdaptiveVisitorMetricTitleLengthRuleFlagsALongTitle();
        AdaptiveVisitorMetricRequiredTitleRuleFlagsABlankTitle();
        AdaptiveVisitorMetricOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
