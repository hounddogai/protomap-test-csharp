namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class TimesheetTierViewChecks
{
    public static void TimesheetTierViewShowsTheStatusInCapitals()
    {
        var item = TimesheetTier.NewTimesheetTier("code-368", "title-933");
        var view = TimesheetTierView.ToTimesheetTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetTierView.ToTimesheetTierViews(new List<TimesheetTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetTierViewShowsTheStatusInCapitals();
    }
}
