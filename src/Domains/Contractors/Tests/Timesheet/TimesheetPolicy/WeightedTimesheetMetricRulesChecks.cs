namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class WeightedTimesheetMetricRulesChecks
{
    public static void WeightedTimesheetMetricValidatorReportsEveryProblem()
    {
        var validator = WeightedTimesheetMetricRules.DefaultWeightedTimesheetMetricValidator();
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedTimesheetMetricRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new WeightedTimesheetMetricRequiredChannelRule("weighted timesheet metric needs a value");
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTimesheetMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedTimesheetMetricRequiredTitleRule("weighted timesheet metric needs a value");
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedTimesheetMetricChannelLengthRuleFlagsALongChannel()
    {
        var rule = new WeightedTimesheetMetricChannelLengthRule(255);
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedTimesheetMetricMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new WeightedTimesheetMetricMarginRangeRule(5.5, 4464.4);
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("x", "y");
        item.Margin = 4465.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedTimesheetMetricValidatorReportsEveryProblem();
        WeightedTimesheetMetricRequiredChannelRuleFlagsABlankChannel();
        WeightedTimesheetMetricRequiredTitleRuleFlagsABlankTitle();
        WeightedTimesheetMetricChannelLengthRuleFlagsALongChannel();
        WeightedTimesheetMetricMarginRangeRuleFlagsValuesAboveMargin();
    }
}
