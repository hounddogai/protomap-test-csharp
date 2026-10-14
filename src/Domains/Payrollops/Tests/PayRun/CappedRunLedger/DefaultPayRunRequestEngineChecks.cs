namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunRequestEngineChecks
{
    public static void DefaultPayRunRequestEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayRunRequestEngine.DefaultDefaultPayRunRequestEngine();
        var item = DefaultPayRunRequest.NewDefaultPayRunRequest("code-642", "label-204");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayRunRequestEngineKeepsResultsWithinBounds();
    }
}
