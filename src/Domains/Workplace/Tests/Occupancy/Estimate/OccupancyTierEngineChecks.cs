namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class OccupancyTierEngineChecks
{
    public static void OccupancyTierEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyTierEngine.DefaultOccupancyTierEngine();
        var item = OccupancyTier.NewOccupancyTier("code-633", "title-778");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyTierEngineKeepsResultsWithinBounds();
    }
}
