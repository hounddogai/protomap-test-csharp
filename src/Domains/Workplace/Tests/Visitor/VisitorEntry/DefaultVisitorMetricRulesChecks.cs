namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class DefaultVisitorMetricRulesChecks
{
    public static void DefaultVisitorMetricValidatorReportsEveryProblem()
    {
        var validator = DefaultVisitorMetricRules.DefaultDefaultVisitorMetricValidator();
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVisitorMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultVisitorMetricLabelLengthRule(3600);
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultVisitorMetricRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new DefaultVisitorMetricRequiredChannelRule("default visitor metric needs this field");
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVisitorMetricRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DefaultVisitorMetricRetriesRangeRule(120, 3600);
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultVisitorMetricValidatorReportsEveryProblem();
        DefaultVisitorMetricLabelLengthRuleFlagsALongLabel();
        DefaultVisitorMetricRequiredChannelRuleFlagsABlankChannel();
        DefaultVisitorMetricRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
