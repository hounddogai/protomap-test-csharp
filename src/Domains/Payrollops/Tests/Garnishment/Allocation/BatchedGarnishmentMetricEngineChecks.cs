namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricEngineChecks
{
    public static void BatchedGarnishmentMetricEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedGarnishmentMetricEngine.DefaultBatchedGarnishmentMetricEngine();
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedGarnishmentMetricEngineKeepsResultsWithinBounds();
    }
}
