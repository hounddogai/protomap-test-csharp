namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyEstimateEngineChecks
{
    public static void OccupancyEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyEstimateEngine.DefaultOccupancyEstimateEngine(Flags.NewRegistry());
        var item = OccupancyEstimate.NewOccupancyEstimate("code-105", "title-460");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyEstimateEngineKeepsResultsWithinBounds();
    }
}
