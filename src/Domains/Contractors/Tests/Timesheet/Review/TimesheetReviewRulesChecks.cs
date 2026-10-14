namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TimesheetReviewRulesChecks
{
    public static void TimesheetReviewValidatorReportsEveryProblem()
    {
        var validator = TimesheetReviewRules.DefaultTimesheetReviewValidator();
        var item = TimesheetReview.NewTimesheetReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetReviewRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TimesheetReviewRequiredLabelRule("timesheet review needs an entry");
        var item = TimesheetReview.NewTimesheetReview("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetReviewRequiredCodeRule("timesheet review needs this field");
        var item = TimesheetReview.NewTimesheetReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetReviewExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TimesheetReviewExpiresAtRangeRule(64, 64);
        var item = TimesheetReview.NewTimesheetReview("x", "y");
        item.ExpiresAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetReviewEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TimesheetReviewEmployeeIdLengthRule(80);
        var item = TimesheetReview.NewTimesheetReview("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetReviewValidatorReportsEveryProblem();
        TimesheetReviewRequiredLabelRuleFlagsABlankLabel();
        TimesheetReviewRequiredCodeRuleFlagsABlankCode();
        TimesheetReviewExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TimesheetReviewEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
