namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class LegacyPoolSegmentEngineChecks
{
    public static void LegacyPoolSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPoolSegmentEngine.DefaultLegacyPoolSegmentEngine(Flags.NewRegistry());
        var item = LegacyPoolSegment.NewLegacyPoolSegment("code-776", "name-420");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPoolSegmentEngineKeepsResultsWithinBounds();
    }
}
