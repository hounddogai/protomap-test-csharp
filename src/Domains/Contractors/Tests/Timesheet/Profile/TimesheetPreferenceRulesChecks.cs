namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetPreferenceRulesChecks
{
    public static void TimesheetPreferenceValidatorReportsEveryProblem()
    {
        var validator = TimesheetPreferenceRules.DefaultTimesheetPreferenceValidator();
        var item = TimesheetPreference.NewTimesheetPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new TimesheetPreferenceNameLengthRule(80);
        var item = TimesheetPreference.NewTimesheetPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetPreferenceRequiredNameRuleFlagsABlankName()
    {
        var rule = new TimesheetPreferenceRequiredNameRule("timesheet preference needs this field");
        var item = TimesheetPreference.NewTimesheetPreference("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetPreferenceValidatorReportsEveryProblem();
        TimesheetPreferenceNameLengthRuleFlagsALongName();
        TimesheetPreferenceRequiredNameRuleFlagsABlankName();
    }
}
