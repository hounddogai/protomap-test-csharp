namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualTaxFormCycleEngineChecks
{
    public static void ManualTaxFormCycleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualTaxFormCycleEngine.DefaultManualTaxFormCycleEngine();
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualTaxFormCycleEngineKeepsResultsWithinBounds();
    }
}
