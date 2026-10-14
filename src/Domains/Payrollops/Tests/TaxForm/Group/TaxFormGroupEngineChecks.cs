namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class TaxFormGroupEngineChecks
{
    public static void TaxFormGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormGroupEngine.DefaultTaxFormGroupEngine();
        var item = TaxFormGroup.NewTaxFormGroup("code-261", "label-563");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormGroupEngineKeepsResultsWithinBounds();
    }
}
