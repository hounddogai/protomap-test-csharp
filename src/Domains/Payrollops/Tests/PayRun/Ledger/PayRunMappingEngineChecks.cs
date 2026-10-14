namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingEngineChecks
{
    public static void PayRunMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunMappingEngine.DefaultPayRunMappingEngine();
        var item = PayRunMapping.NewPayRunMapping("code-153", "name-757");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunMappingEngineKeepsResultsWithinBounds();
    }
}
