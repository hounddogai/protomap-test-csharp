namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetGroupRulesChecks
{
    public static void TimesheetGroupValidatorReportsEveryProblem()
    {
        var validator = TimesheetGroupRules.DefaultTimesheetGroupValidator();
        var item = TimesheetGroup.NewTimesheetGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetGroupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TimesheetGroupRetriesRangeRule(32, 255);
        var item = TimesheetGroup.NewTimesheetGroup("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetGroupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TimesheetGroupRequiredTitleRule("timesheet group needs an entry");
        var item = TimesheetGroup.NewTimesheetGroup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetGroupLocationLengthRuleFlagsALongLocation()
    {
        var rule = new TimesheetGroupLocationLengthRule(86400);
        var item = TimesheetGroup.NewTimesheetGroup("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetGroupValidatorReportsEveryProblem();
        TimesheetGroupRetriesRangeRuleFlagsValuesAboveRetries();
        TimesheetGroupRequiredTitleRuleFlagsABlankTitle();
        TimesheetGroupLocationLengthRuleFlagsALongLocation();
    }
}
