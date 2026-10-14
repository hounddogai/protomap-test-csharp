namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>Checks of pay policy code, run by the project's runner.</summary>
public static class BatchedRetroPayPlanEngineChecks
{
    public static void BatchedRetroPayPlanEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedRetroPayPlanEngine.DefaultBatchedRetroPayPlanEngine();
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedRetroPayPlanEngineKeepsResultsWithinBounds();
    }
}
