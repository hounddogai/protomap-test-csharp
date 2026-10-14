namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PendingPoolAllocationEngineChecks
{
    public static void PendingPoolAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPoolAllocationEngine.DefaultPendingPoolAllocationEngine();
        var item = PendingPoolAllocation.NewPendingPoolAllocation("code-485", "title-634");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPoolAllocationEngineKeepsResultsWithinBounds();
    }
}
