namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

/// <summary>Checks of occupancy draft code, run by the project's runner.</summary>
public static class RollingOccupancyAllocationEngineChecks
{
    public static void RollingOccupancyAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = RollingOccupancyAllocationEngine.DefaultRollingOccupancyAllocationEngine(Flags.NewRegistry());
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("code-608", "name-94");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingOccupancyAllocationEngineKeepsResultsWithinBounds();
    }
}
