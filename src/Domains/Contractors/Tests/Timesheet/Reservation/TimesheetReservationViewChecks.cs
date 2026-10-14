namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetReservationViewChecks
{
    public static void TimesheetReservationViewShowsTheStatusInCapitals()
    {
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
        var view = TimesheetReservationView.ToTimesheetReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetReservationView.ToTimesheetReservationViews(new List<TimesheetReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetReservationViewShowsTheStatusInCapitals();
    }
}
