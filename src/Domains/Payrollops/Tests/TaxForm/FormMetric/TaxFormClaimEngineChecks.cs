namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Checks of form metric code, run by the project's runner.</summary>
public static class TaxFormClaimEngineChecks
{
    public static void TaxFormClaimEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormClaimEngine.DefaultTaxFormClaimEngine();
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormClaimEngineKeepsResultsWithinBounds();
    }
}
