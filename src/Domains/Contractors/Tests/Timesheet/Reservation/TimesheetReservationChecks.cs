namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetReservationChecks
{
    public static void TimesheetReservationStartsInOpen()
    {
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
        Check.Equal(item.Status, TimesheetReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetReservationMovesToResolved()
    {
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
        try
        {
            item.MoveTo(TimesheetReservationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetReservationStatus.Resolved, "status");
    }

    public static void TimesheetReservationRejectsMovesFromArchived()
    {
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
        item.Status = TimesheetReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(TimesheetReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        TimesheetReservationStartsInOpen();
        TimesheetReservationMovesToResolved();
        TimesheetReservationRejectsMovesFromArchived();
    }
}
