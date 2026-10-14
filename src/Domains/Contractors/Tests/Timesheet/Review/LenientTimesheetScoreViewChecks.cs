namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreViewChecks
{
    public static void LenientTimesheetScoreViewShowsTheStatusInCapitals()
    {
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
        var view = LenientTimesheetScoreView.ToLenientTimesheetScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientTimesheetScoreView.ToLenientTimesheetScoreViews(new List<LenientTimesheetScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientTimesheetScoreViewShowsTheStatusInCapitals();
    }
}
