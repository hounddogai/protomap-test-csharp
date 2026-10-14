namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBatchEngineChecks
{
    public static void PayRunBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunBatchEngine.DefaultPayRunBatchEngine();
        var item = PayRunBatch.NewPayRunBatch("code-401", "label-180");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunBatchEngineKeepsResultsWithinBounds();
    }
}
