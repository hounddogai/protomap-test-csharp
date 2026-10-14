namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveTaxFormThresholdEngineChecks
{
    public static void AdaptiveTaxFormThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTaxFormThresholdEngine.DefaultAdaptiveTaxFormThresholdEngine(Flags.NewRegistry());
        var item = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold("code-426", "name-162");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormThresholdEngineKeepsResultsWithinBounds();
    }
}
