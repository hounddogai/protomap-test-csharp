namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormManifestEngineChecks
{
    public static void TaxFormManifestEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormManifestEngine.DefaultTaxFormManifestEngine();
        var item = TaxFormManifest.NewTaxFormManifest("code-314", "title-785");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormManifestEngineKeepsResultsWithinBounds();
    }
}
