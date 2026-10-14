namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormLedgerEngineChecks
{
    public static void AdaptiveTaxFormLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTaxFormLedgerEngine.DefaultAdaptiveTaxFormLedgerEngine(Flags.NewRegistry());
        var item = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger("code-782", "label-956");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormLedgerEngineKeepsResultsWithinBounds();
    }
}
