namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationEngineChecks
{
    public static void PendingHolidayReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PendingHolidayReservationEngine.DefaultPendingHolidayReservationEngine(Flags.NewRegistry());
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingHolidayReservationEngineKeepsResultsWithinBounds();
    }
}
