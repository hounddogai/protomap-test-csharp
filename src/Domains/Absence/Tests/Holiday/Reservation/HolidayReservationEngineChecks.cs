namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class HolidayReservationEngineChecks
{
    public static void HolidayReservationEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayReservationEngine.DefaultHolidayReservationEngine();
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayReservationEngineKeepsResultsWithinBounds();
    }
}
