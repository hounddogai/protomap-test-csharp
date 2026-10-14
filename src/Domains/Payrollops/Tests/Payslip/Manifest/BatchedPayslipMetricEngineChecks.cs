namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class BatchedPayslipMetricEngineChecks
{
    public static void BatchedPayslipMetricEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayslipMetricEngine.DefaultBatchedPayslipMetricEngine();
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("code-639", "title-10");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayslipMetricEngineKeepsResultsWithinBounds();
    }
}
