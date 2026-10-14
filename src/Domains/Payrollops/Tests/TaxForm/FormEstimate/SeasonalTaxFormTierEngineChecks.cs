namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class SeasonalTaxFormTierEngineChecks
{
    public static void SeasonalTaxFormTierEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalTaxFormTierEngine.DefaultSeasonalTaxFormTierEngine(Flags.NewRegistry());
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalTaxFormTierEngineKeepsResultsWithinBounds();
    }
}
