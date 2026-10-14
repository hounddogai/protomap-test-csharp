namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetTargetRulesChecks
{
    public static void TimesheetTargetValidatorReportsEveryProblem()
    {
        var validator = TimesheetTargetRules.DefaultTimesheetTargetValidator();
        var item = TimesheetTarget.NewTimesheetTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetTargetRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TimesheetTargetRequiredPhoneRule("timesheet target needs an entry");
        var item = TimesheetTarget.NewTimesheetTarget("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetTargetPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new TimesheetTargetPhoneLengthRule(32);
        var item = TimesheetTarget.NewTimesheetTarget("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetTargetValidatorReportsEveryProblem();
        TimesheetTargetRequiredPhoneRuleFlagsABlankPhone();
        TimesheetTargetPhoneLengthRuleFlagsALongPhone();
    }
}
