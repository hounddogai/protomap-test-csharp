namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantMetricEngineChecks
{
    public static void EquityGrantMetricEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantMetricEngine.DefaultEquityGrantMetricEngine();
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantMetricEngineKeepsResultsWithinBounds();
    }
}
