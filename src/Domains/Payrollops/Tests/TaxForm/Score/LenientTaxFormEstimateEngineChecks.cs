namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientTaxFormEstimateEngineChecks
{
    public static void LenientTaxFormEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTaxFormEstimateEngine.DefaultLenientTaxFormEstimateEngine(Flags.NewRegistry());
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("code-446", "label-118");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTaxFormEstimateEngineKeepsResultsWithinBounds();
    }
}
