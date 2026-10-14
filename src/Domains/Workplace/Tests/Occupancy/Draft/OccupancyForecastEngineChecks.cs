namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyForecastEngineChecks
{
    public static void OccupancyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyForecastEngine.DefaultOccupancyForecastEngine();
        var item = OccupancyForecast.NewOccupancyForecast("code-24", "title-518");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyForecastEngineKeepsResultsWithinBounds();
    }
}
