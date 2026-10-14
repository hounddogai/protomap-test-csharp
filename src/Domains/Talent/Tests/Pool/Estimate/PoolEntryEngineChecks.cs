namespace Acme.Workforce.Talent.Tests.Pool.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PoolEntryEngineChecks
{
    public static void PoolEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PoolEntryEngine.DefaultPoolEntryEngine(Flags.NewRegistry());
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolEntryEngineKeepsResultsWithinBounds();
    }
}
