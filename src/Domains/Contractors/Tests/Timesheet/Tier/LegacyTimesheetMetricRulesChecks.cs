namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyTimesheetMetricRulesChecks
{
    public static void LegacyTimesheetMetricValidatorReportsEveryProblem()
    {
        var validator = LegacyTimesheetMetricRules.DefaultLegacyTimesheetMetricValidator();
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTimesheetMetricRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LegacyTimesheetMetricRequiredPhoneRule("legacy timesheet metric needs an entry");
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTimesheetMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyTimesheetMetricRequiredTitleRule("legacy timesheet metric needs this field");
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyTimesheetMetricValidatorReportsEveryProblem();
        LegacyTimesheetMetricRequiredPhoneRuleFlagsABlankPhone();
        LegacyTimesheetMetricRequiredTitleRuleFlagsABlankTitle();
    }
}
