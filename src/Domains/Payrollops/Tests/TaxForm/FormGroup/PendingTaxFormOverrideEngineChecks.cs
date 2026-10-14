namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideEngineChecks
{
    public static void PendingTaxFormOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PendingTaxFormOverrideEngine.DefaultPendingTaxFormOverrideEngine(Flags.NewRegistry());
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingTaxFormOverrideEngineKeepsResultsWithinBounds();
    }
}
