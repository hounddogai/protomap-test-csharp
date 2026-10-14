namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class HolidayThresholdEngineChecks
{
    public static void HolidayThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayThresholdEngine.DefaultHolidayThresholdEngine(Flags.NewRegistry());
        var item = HolidayThreshold.NewHolidayThreshold("code-315", "title-572");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayThresholdEngineKeepsResultsWithinBounds();
    }
}
