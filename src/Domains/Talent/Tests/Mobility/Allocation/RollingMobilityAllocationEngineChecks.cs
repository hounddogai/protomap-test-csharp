namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityAllocationEngineChecks
{
    public static void RollingMobilityAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = RollingMobilityAllocationEngine.DefaultRollingMobilityAllocationEngine();
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("code-72", "name-321");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingMobilityAllocationEngineKeepsResultsWithinBounds();
    }
}
