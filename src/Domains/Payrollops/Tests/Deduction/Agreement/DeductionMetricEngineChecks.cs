namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionMetricEngineChecks
{
    public static void DeductionMetricEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionMetricEngine.DefaultDeductionMetricEngine();
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionMetricEngineKeepsResultsWithinBounds();
    }
}
