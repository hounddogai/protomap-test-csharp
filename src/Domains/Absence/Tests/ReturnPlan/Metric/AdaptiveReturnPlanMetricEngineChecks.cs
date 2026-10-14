namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanMetricEngineChecks
{
    public static void AdaptiveReturnPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveReturnPlanMetricEngine.DefaultAdaptiveReturnPlanMetricEngine();
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("code-589", "title-185");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanMetricEngineKeepsResultsWithinBounds();
    }
}
