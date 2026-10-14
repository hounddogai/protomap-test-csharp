namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricEngineChecks
{
    public static void DefaultRefreshMetricEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultRefreshMetricEngine.DefaultDefaultRefreshMetricEngine(Flags.NewRegistry());
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultRefreshMetricEngineKeepsResultsWithinBounds();
    }
}
