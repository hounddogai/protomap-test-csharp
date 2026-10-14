namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedOccupancyForecastEngineChecks
{
    public static void WeightedOccupancyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedOccupancyForecastEngine.DefaultWeightedOccupancyForecastEngine(Flags.NewRegistry());
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("code-661", "name-625");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedOccupancyForecastEngineKeepsResultsWithinBounds();
    }
}
