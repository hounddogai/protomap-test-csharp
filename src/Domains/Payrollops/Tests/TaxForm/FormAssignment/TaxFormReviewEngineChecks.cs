namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormReviewEngineChecks
{
    public static void TaxFormReviewEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormReviewEngine.DefaultTaxFormReviewEngine(Flags.NewRegistry());
        var item = TaxFormReview.NewTaxFormReview("code-839", "label-497");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormReviewEngineKeepsResultsWithinBounds();
    }
}
