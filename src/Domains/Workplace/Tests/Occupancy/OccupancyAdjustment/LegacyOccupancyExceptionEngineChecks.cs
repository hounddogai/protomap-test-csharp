namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class LegacyOccupancyExceptionEngineChecks
{
    public static void LegacyOccupancyExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyOccupancyExceptionEngine.DefaultLegacyOccupancyExceptionEngine();
        var item = LegacyOccupancyException.NewLegacyOccupancyException("code-784", "title-249");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyOccupancyExceptionEngineKeepsResultsWithinBounds();
    }
}
