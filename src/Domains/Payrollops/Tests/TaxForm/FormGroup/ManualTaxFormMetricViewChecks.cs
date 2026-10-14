namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class ManualTaxFormMetricViewChecks
{
    public static void ManualTaxFormMetricViewShowsTheStatusInCapitals()
    {
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("code-797", "title-380");
        var view = ManualTaxFormMetricView.ToManualTaxFormMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualTaxFormMetricView.ToManualTaxFormMetricViews(new List<ManualTaxFormMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualTaxFormMetricViewShowsTheStatusInCapitals();
    }
}
