namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class TieredPoolEntryEngineChecks
{
    public static void TieredPoolEntryEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPoolEntryEngine.DefaultTieredPoolEntryEngine(Flags.NewRegistry());
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPoolEntryEngineKeepsResultsWithinBounds();
    }
}
