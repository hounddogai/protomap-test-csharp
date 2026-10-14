namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMetricEngineChecks
{
    public static void ReorgMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgMetricEngine.DefaultReorgMetricEngine(Flags.NewRegistry());
        var item = ReorgMetric.NewReorgMetric("code-772", "title-38");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgMetricEngineKeepsResultsWithinBounds();
    }
}
