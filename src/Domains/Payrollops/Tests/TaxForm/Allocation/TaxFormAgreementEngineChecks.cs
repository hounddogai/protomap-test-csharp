namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAgreementEngineChecks
{
    public static void TaxFormAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormAgreementEngine.DefaultTaxFormAgreementEngine();
        var item = TaxFormAgreement.NewTaxFormAgreement("code-74", "title-745");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormAgreementEngineKeepsResultsWithinBounds();
    }
}
