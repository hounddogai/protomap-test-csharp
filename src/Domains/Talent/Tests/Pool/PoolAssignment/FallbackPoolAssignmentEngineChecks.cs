namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class FallbackPoolAssignmentEngineChecks
{
    public static void FallbackPoolAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPoolAssignmentEngine.DefaultFallbackPoolAssignmentEngine(Flags.NewRegistry());
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("code-293", "name-132");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPoolAssignmentEngineKeepsResultsWithinBounds();
    }
}
