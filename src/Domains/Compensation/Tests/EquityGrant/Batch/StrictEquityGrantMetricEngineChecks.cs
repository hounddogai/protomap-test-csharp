namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class StrictEquityGrantMetricEngineChecks
{
    public static void StrictEquityGrantMetricEngineKeepsResultsWithinBounds()
    {
        var engine = StrictEquityGrantMetricEngine.DefaultStrictEquityGrantMetricEngine();
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("code-840", "title-14");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictEquityGrantMetricEngineKeepsResultsWithinBounds();
    }
}
