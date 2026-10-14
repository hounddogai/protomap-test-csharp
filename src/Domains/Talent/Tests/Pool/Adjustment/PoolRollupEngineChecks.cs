namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRollupEngineChecks
{
    public static void PoolRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PoolRollupEngine.DefaultPoolRollupEngine(Flags.NewRegistry());
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolRollupEngineKeepsResultsWithinBounds();
    }
}
