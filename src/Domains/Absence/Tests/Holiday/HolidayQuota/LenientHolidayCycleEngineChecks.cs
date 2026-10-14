namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class LenientHolidayCycleEngineChecks
{
    public static void LenientHolidayCycleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientHolidayCycleEngine.DefaultLenientHolidayCycleEngine(Flags.NewRegistry());
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientHolidayCycleEngineKeepsResultsWithinBounds();
    }
}
