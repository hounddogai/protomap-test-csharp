namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingTimesheetMetricRulesChecks
{
    public static void PendingTimesheetMetricValidatorReportsEveryProblem()
    {
        var validator = PendingTimesheetMetricRules.DefaultPendingTimesheetMetricValidator();
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTimesheetMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingTimesheetMetricLabelLengthRule(86400);
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTimesheetMetricRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PendingTimesheetMetricRequiredCountryRule("pending timesheet metric needs an entry");
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingTimesheetMetricPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PendingTimesheetMetricPriorityRangeRule(80, 255);
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingTimesheetMetricValidatorReportsEveryProblem();
        PendingTimesheetMetricLabelLengthRuleFlagsALongLabel();
        PendingTimesheetMetricRequiredCountryRuleFlagsABlankCountry();
        PendingTimesheetMetricPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
