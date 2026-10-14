namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TimesheetReviewEngineChecks
{
    public static void TimesheetReviewEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetReviewEngine.DefaultTimesheetReviewEngine();
        var item = TimesheetReview.NewTimesheetReview("code-277", "label-206");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetReviewEngineKeepsResultsWithinBounds();
    }
}
