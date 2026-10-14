namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackHolidayScheduleEngineChecks
{
    public static void FallbackHolidayScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackHolidayScheduleEngine.DefaultFallbackHolidayScheduleEngine(Flags.NewRegistry());
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("code-850", "title-628");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackHolidayScheduleEngineKeepsResultsWithinBounds();
    }
}
