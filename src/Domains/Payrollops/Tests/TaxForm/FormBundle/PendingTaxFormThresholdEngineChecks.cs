namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class PendingTaxFormThresholdEngineChecks
{
    public static void PendingTaxFormThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = PendingTaxFormThresholdEngine.DefaultPendingTaxFormThresholdEngine(Flags.NewRegistry());
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("code-390", "label-516");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingTaxFormThresholdEngineKeepsResultsWithinBounds();
    }
}
