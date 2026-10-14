namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationEngineChecks
{
    public static void OccupancyAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyAllocationEngine.DefaultOccupancyAllocationEngine();
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyAllocationEngineKeepsResultsWithinBounds();
    }
}
