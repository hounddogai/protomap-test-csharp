namespace Acme.Workforce.Payrollops.Tests.PayRun.Variance;

using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingPayRunAllocationEngineChecks
{
    public static void PendingPayRunAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPayRunAllocationEngine.DefaultPendingPayRunAllocationEngine();
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("code-908", "name-89");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPayRunAllocationEngineKeepsResultsWithinBounds();
    }
}
