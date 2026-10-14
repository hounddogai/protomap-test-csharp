namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TimesheetReviewViewChecks
{
    public static void TimesheetReviewViewShowsTheStatusInCapitals()
    {
        var item = TimesheetReview.NewTimesheetReview("code-277", "label-206");
        var view = TimesheetReviewView.ToTimesheetReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetReviewView.ToTimesheetReviewViews(new List<TimesheetReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetReviewViewShowsTheStatusInCapitals();
    }
}
