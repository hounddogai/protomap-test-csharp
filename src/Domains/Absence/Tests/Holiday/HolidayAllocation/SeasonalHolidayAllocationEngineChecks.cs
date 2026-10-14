namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class SeasonalHolidayAllocationEngineChecks
{
    public static void SeasonalHolidayAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalHolidayAllocationEngine.DefaultSeasonalHolidayAllocationEngine();
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("code-541", "name-720");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalHolidayAllocationEngineKeepsResultsWithinBounds();
    }
}
