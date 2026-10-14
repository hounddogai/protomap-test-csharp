namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class ManualTaxFormMetricEngineChecks
{
    public static void ManualTaxFormMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ManualTaxFormMetricEngine.DefaultManualTaxFormMetricEngine();
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("code-797", "title-380");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualTaxFormMetricEngineKeepsResultsWithinBounds();
    }
}
