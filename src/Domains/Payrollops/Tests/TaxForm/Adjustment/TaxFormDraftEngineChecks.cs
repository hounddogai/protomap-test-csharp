namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormDraftEngineChecks
{
    public static void TaxFormDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormDraftEngine.DefaultTaxFormDraftEngine(Flags.NewRegistry());
        var item = TaxFormDraft.NewTaxFormDraft("code-297", "name-423");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormDraftEngineKeepsResultsWithinBounds();
    }
}
