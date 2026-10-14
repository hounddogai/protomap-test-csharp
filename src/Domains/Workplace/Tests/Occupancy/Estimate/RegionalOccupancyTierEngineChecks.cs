namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RegionalOccupancyTierEngineChecks
{
    public static void RegionalOccupancyTierEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalOccupancyTierEngine.DefaultRegionalOccupancyTierEngine();
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("code-668", "name-299");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalOccupancyTierEngineKeepsResultsWithinBounds();
    }
}
