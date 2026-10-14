namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class HolidayEstimateEngineChecks
{
    public static void HolidayEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayEstimateEngine.DefaultHolidayEstimateEngine(Flags.NewRegistry());
        var item = HolidayEstimate.NewHolidayEstimate("code-820", "title-100");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayEstimateEngineKeepsResultsWithinBounds();
    }
}
