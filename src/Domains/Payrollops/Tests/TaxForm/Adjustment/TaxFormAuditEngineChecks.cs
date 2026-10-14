namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormAuditEngineChecks
{
    public static void TaxFormAuditEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormAuditEngine.DefaultTaxFormAuditEngine(Flags.NewRegistry());
        var item = TaxFormAudit.NewTaxFormAudit("code-560", "title-721");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormAuditEngineKeepsResultsWithinBounds();
    }
}
