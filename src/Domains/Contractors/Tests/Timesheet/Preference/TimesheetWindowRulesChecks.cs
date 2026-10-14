namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TimesheetWindowRulesChecks
{
    public static void TimesheetWindowValidatorReportsEveryProblem()
    {
        var validator = TimesheetWindowRules.DefaultTimesheetWindowValidator();
        var item = TimesheetWindow.NewTimesheetWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetWindowRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new TimesheetWindowRequiredTimezoneRule("timesheet window needs this field");
        var item = TimesheetWindow.NewTimesheetWindow("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetWindowVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new TimesheetWindowVersionRangeRule(32, 64);
        var item = TimesheetWindow.NewTimesheetWindow("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetWindowTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new TimesheetWindowTimezoneLengthRule(86400);
        var item = TimesheetWindow.NewTimesheetWindow("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetWindowScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TimesheetWindowScoreRangeRule(3.3, 2884.9);
        var item = TimesheetWindow.NewTimesheetWindow("x", "y");
        item.Score = 2886.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TimesheetWindowValidatorReportsEveryProblem();
        TimesheetWindowRequiredTimezoneRuleFlagsABlankTimezone();
        TimesheetWindowVersionRangeRuleFlagsValuesAboveVersion();
        TimesheetWindowTimezoneLengthRuleFlagsALongTimezone();
        TimesheetWindowScoreRangeRuleFlagsValuesAboveScore();
    }
}
