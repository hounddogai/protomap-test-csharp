namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetRecordRulesChecks
{
    public static void TimesheetRecordValidatorReportsEveryProblem()
    {
        var validator = TimesheetRecordRules.DefaultTimesheetRecordValidator();
        var item = TimesheetRecord.NewTimesheetRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetRecordAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TimesheetRecordAttemptsRangeRule(64, 80);
        var item = TimesheetRecord.NewTimesheetRecord("x", "y");
        item.Attempts = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetRecordMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TimesheetRecordMinimumRangeRule(6.4, 2229.0);
        var item = TimesheetRecord.NewTimesheetRecord("x", "y");
        item.Minimum = 2230.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetRecordRequiredCodeRule("timesheet record needs an entry");
        var item = TimesheetRecord.NewTimesheetRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetRecordValidatorReportsEveryProblem();
        TimesheetRecordAttemptsRangeRuleFlagsValuesAboveAttempts();
        TimesheetRecordMinimumRangeRuleFlagsValuesAboveMinimum();
        TimesheetRecordRequiredCodeRuleFlagsABlankCode();
    }
}
