namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualTaxFormAuditEngineChecks
{
    public static void ManualTaxFormAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ManualTaxFormAuditEngine.DefaultManualTaxFormAuditEngine();
        var item = ManualTaxFormAudit.NewManualTaxFormAudit("code-159", "name-971");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualTaxFormAuditEngineKeepsResultsWithinBounds();
    }
}
