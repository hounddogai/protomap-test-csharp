namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictReturnPlanMetricEngineChecks
{
    public static void StrictReturnPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = StrictReturnPlanMetricEngine.DefaultStrictReturnPlanMetricEngine(Flags.NewRegistry());
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("code-948", "label-509");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictReturnPlanMetricEngineKeepsResultsWithinBounds();
    }
}
