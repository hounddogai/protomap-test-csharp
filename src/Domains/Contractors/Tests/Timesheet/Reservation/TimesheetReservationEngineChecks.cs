namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetReservationEngineChecks
{
    public static void TimesheetReservationEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetReservationEngine.DefaultTimesheetReservationEngine();
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetReservationEngineKeepsResultsWithinBounds();
    }
}
