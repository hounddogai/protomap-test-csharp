namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class LegacyHolidayScheduleEngineChecks
{
    public static void LegacyHolidayScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyHolidayScheduleEngine.DefaultLegacyHolidayScheduleEngine(Flags.NewRegistry());
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("code-283", "label-748");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyHolidayScheduleEngineKeepsResultsWithinBounds();
    }
}
