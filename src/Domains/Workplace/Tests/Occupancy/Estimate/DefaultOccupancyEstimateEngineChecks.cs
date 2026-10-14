namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class DefaultOccupancyEstimateEngineChecks
{
    public static void DefaultOccupancyEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultOccupancyEstimateEngine.DefaultDefaultOccupancyEstimateEngine();
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("code-483", "label-972");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultOccupancyEstimateEngineKeepsResultsWithinBounds();
    }
}
