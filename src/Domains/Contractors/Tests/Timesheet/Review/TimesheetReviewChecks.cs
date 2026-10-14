namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TimesheetReviewChecks
{
    public static void TimesheetReviewStartsInCreated()
    {
        var item = TimesheetReview.NewTimesheetReview("code-277", "label-206");
        Check.Equal(item.Status, TimesheetReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetReviewMovesToValidated()
    {
        var item = TimesheetReview.NewTimesheetReview("code-277", "label-206");
        try
        {
            item.MoveTo(TimesheetReviewStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetReviewStatus.Validated, "status");
    }

    public static void TimesheetReviewRejectsMovesFromRetired()
    {
        var item = TimesheetReview.NewTimesheetReview("code-277", "label-206");
        item.Status = TimesheetReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetReviewStartsInCreated();
        TimesheetReviewMovesToValidated();
        TimesheetReviewRejectsMovesFromRetired();
    }
}
