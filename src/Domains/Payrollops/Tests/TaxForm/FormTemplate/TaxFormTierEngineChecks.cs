namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormTierEngineChecks
{
    public static void TaxFormTierEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormTierEngine.DefaultTaxFormTierEngine();
        var item = TaxFormTier.NewTaxFormTier("code-187", "name-469");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormTierEngineKeepsResultsWithinBounds();
    }
}
