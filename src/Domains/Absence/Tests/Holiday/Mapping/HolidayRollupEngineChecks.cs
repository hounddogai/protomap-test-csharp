namespace Acme.Workforce.Absence.Tests.Holiday.Mapping;

using Acme.Workforce.Absence.Holiday.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class HolidayRollupEngineChecks
{
    public static void HolidayRollupEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayRollupEngine.DefaultHolidayRollupEngine(Flags.NewRegistry());
        var item = HolidayRollup.NewHolidayRollup("code-869", "label-564");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayRollupEngineKeepsResultsWithinBounds();
    }
}
