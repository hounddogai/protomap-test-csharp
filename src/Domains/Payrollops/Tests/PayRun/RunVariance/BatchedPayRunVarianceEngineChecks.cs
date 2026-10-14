namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class BatchedPayRunVarianceEngineChecks
{
    public static void BatchedPayRunVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayRunVarianceEngine.DefaultBatchedPayRunVarianceEngine();
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("code-313", "title-46");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayRunVarianceEngineKeepsResultsWithinBounds();
    }
}
