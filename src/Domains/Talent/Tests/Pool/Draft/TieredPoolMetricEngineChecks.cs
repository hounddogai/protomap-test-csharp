namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TieredPoolMetricEngineChecks
{
    public static void TieredPoolMetricEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPoolMetricEngine.DefaultTieredPoolMetricEngine(Flags.NewRegistry());
        var item = TieredPoolMetric.NewTieredPoolMetric("code-683", "label-94");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPoolMetricEngineKeepsResultsWithinBounds();
    }
}
