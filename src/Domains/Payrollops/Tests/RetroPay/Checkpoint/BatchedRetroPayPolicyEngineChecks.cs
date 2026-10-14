namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedRetroPayPolicyEngineChecks
{
    public static void BatchedRetroPayPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedRetroPayPolicyEngine.DefaultBatchedRetroPayPolicyEngine(Flags.NewRegistry());
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedRetroPayPolicyEngineKeepsResultsWithinBounds();
    }
}
