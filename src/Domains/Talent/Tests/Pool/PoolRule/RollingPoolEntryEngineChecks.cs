namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class RollingPoolEntryEngineChecks
{
    public static void RollingPoolEntryEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPoolEntryEngine.DefaultRollingPoolEntryEngine(Flags.NewRegistry());
        var item = RollingPoolEntry.NewRollingPoolEntry("code-615", "title-533");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPoolEntryEngineKeepsResultsWithinBounds();
    }
}
