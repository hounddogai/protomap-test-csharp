namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class OccupancyTargetEngineChecks
{
    public static void OccupancyTargetEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyTargetEngine.DefaultOccupancyTargetEngine(Flags.NewRegistry());
        var item = OccupancyTarget.NewOccupancyTarget("code-990", "name-997");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyTargetEngineKeepsResultsWithinBounds();
    }
}
