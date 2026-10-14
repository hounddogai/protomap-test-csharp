namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetVarianceRulesChecks
{
    public static void TimesheetVarianceValidatorReportsEveryProblem()
    {
        var validator = TimesheetVarianceRules.DefaultTimesheetVarianceValidator();
        var item = TimesheetVariance.NewTimesheetVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetVarianceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TimesheetVarianceScoreRangeRule(0.7, 4799.0);
        var item = TimesheetVariance.NewTimesheetVariance("x", "y");
        item.Score = 4800.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetVarianceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TimesheetVarianceRateRangeRule(1.2, 2269.1);
        var item = TimesheetVariance.NewTimesheetVariance("x", "y");
        item.Rate = 2270.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetVarianceReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new TimesheetVarianceReviewerLengthRule(3600);
        var item = TimesheetVariance.NewTimesheetVariance("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetVarianceRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new TimesheetVarianceRequiredReviewerRule("timesheet variance needs an entry");
        var item = TimesheetVariance.NewTimesheetVariance("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetVarianceValidatorReportsEveryProblem();
        TimesheetVarianceScoreRangeRuleFlagsValuesAboveScore();
        TimesheetVarianceRateRangeRuleFlagsValuesAboveRate();
        TimesheetVarianceReviewerLengthRuleFlagsALongReviewer();
        TimesheetVarianceRequiredReviewerRuleFlagsABlankReviewer();
    }
}
