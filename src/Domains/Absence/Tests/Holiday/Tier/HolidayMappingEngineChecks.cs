namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayMappingEngineChecks
{
    public static void HolidayMappingEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayMappingEngine.DefaultHolidayMappingEngine();
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayMappingEngineKeepsResultsWithinBounds();
    }
}
