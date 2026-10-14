namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TaxFormRequestEngineChecks
{
    public static void TaxFormRequestEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormRequestEngine.DefaultTaxFormRequestEngine();
        var item = TaxFormRequest.NewTaxFormRequest("code-346", "label-332");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormRequestEngineKeepsResultsWithinBounds();
    }
}
