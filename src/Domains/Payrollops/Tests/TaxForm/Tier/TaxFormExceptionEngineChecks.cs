namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TaxFormExceptionEngineChecks
{
    public static void TaxFormExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormExceptionEngine.DefaultTaxFormExceptionEngine();
        var item = TaxFormException.NewTaxFormException("code-232", "title-21");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormExceptionEngineKeepsResultsWithinBounds();
    }
}
