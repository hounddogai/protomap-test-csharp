namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccrualMetricEngineChecks
{
    public static void AccrualMetricEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualMetricEngine.DefaultAccrualMetricEngine();
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualMetricEngineKeepsResultsWithinBounds();
    }
}
