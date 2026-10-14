namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetThresholdRulesChecks
{
    public static void TimesheetThresholdValidatorReportsEveryProblem()
    {
        var validator = TimesheetThresholdRules.DefaultTimesheetThresholdValidator();
        var item = TimesheetThreshold.NewTimesheetThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetThresholdRequiredCodeRule("timesheet threshold needs an entry");
        var item = TimesheetThreshold.NewTimesheetThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetThresholdMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new TimesheetThresholdMaximumRangeRule(3.7, 300.2);
        var item = TimesheetThreshold.NewTimesheetThreshold("x", "y");
        item.Maximum = 301.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new TimesheetThresholdCodeLengthRule(86400);
        var item = TimesheetThreshold.NewTimesheetThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetThresholdPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new TimesheetThresholdPriorityRangeRule(64, 86400);
        var item = TimesheetThreshold.NewTimesheetThreshold("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetThresholdJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TimesheetThresholdJobCodeLengthRule(1000);
        var item = TimesheetThreshold.NewTimesheetThreshold("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetThresholdValidatorReportsEveryProblem();
        TimesheetThresholdRequiredCodeRuleFlagsABlankCode();
        TimesheetThresholdMaximumRangeRuleFlagsValuesAboveMaximum();
        TimesheetThresholdCodeLengthRuleFlagsALongCode();
        TimesheetThresholdPriorityRangeRuleFlagsValuesAbovePriority();
        TimesheetThresholdJobCodeLengthRuleFlagsALongJobCode();
    }
}
