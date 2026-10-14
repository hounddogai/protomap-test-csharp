namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMetricEngineChecks
{
    public static void TaxFormMetricEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormMetricEngine.DefaultTaxFormMetricEngine(Flags.NewRegistry());
        var item = TaxFormMetric.NewTaxFormMetric("code-252", "label-795");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormMetricEngineKeepsResultsWithinBounds();
    }
}
