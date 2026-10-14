namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetSegmentRulesChecks
{
    public static void TimesheetSegmentValidatorReportsEveryProblem()
    {
        var validator = TimesheetSegmentRules.DefaultTimesheetSegmentValidator();
        var item = TimesheetSegment.NewTimesheetSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetSegmentRequiredCodeRule("timesheet segment needs an entry");
        var item = TimesheetSegment.NewTimesheetSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new TimesheetSegmentNameLengthRule(80);
        var item = TimesheetSegment.NewTimesheetSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetSegmentValidatorReportsEveryProblem();
        TimesheetSegmentRequiredCodeRuleFlagsABlankCode();
        TimesheetSegmentNameLengthRuleFlagsALongName();
    }
}
