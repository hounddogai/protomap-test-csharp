namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetPlanViewChecks
{
    public static void TimesheetPlanViewShowsTheStatusInCapitals()
    {
        var item = TimesheetPlan.NewTimesheetPlan("code-433", "title-301");
        var view = TimesheetPlanView.ToTimesheetPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetPlanView.ToTimesheetPlanViews(new List<TimesheetPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetPlanViewShowsTheStatusInCapitals();
    }
}
