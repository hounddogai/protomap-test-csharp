namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TaxFormBatchEngineChecks
{
    public static void TaxFormBatchEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormBatchEngine.DefaultTaxFormBatchEngine();
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormBatchEngineKeepsResultsWithinBounds();
    }
}
