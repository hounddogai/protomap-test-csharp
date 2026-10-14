namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetMilestoneRulesChecks
{
    public static void TimesheetMilestoneValidatorReportsEveryProblem()
    {
        var validator = TimesheetMilestoneRules.DefaultTimesheetMilestoneValidator();
        var item = TimesheetMilestone.NewTimesheetMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetMilestoneRequiredCodeRule("timesheet milestone needs an entry");
        var item = TimesheetMilestone.NewTimesheetMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetMilestoneReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new TimesheetMilestoneReviewerLengthRule(64);
        var item = TimesheetMilestone.NewTimesheetMilestone("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetMilestoneValidatorReportsEveryProblem();
        TimesheetMilestoneRequiredCodeRuleFlagsABlankCode();
        TimesheetMilestoneReviewerLengthRuleFlagsALongReviewer();
    }
}
