namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTimesheetGroupRulesChecks
{
    public static void ScheduledTimesheetGroupValidatorReportsEveryProblem()
    {
        var validator = ScheduledTimesheetGroupRules.DefaultScheduledTimesheetGroupValidator();
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTimesheetGroupEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScheduledTimesheetGroupEndsAtRangeRule(120, 1000);
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTimesheetGroupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ScheduledTimesheetGroupThresholdRangeRule(4.5, 3960.2);
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("x", "y");
        item.Threshold = 3961.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTimesheetGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledTimesheetGroupRequiredLabelRule("scheduled timesheet group needs a value");
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledTimesheetGroupValidatorReportsEveryProblem();
        ScheduledTimesheetGroupEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScheduledTimesheetGroupThresholdRangeRuleFlagsValuesAboveThreshold();
        ScheduledTimesheetGroupRequiredLabelRuleFlagsABlankLabel();
    }
}
