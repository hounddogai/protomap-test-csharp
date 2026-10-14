namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunCycleEngineChecks
{
    public static void PayRunCycleEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunCycleEngine.DefaultPayRunCycleEngine();
        var item = PayRunCycle.NewPayRunCycle("code-153", "name-961");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunCycleEngineKeepsResultsWithinBounds();
    }
}
