namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMetricViewChecks
{
    public static void TaxFormMetricViewShowsTheStatusInCapitals()
    {
        var item = TaxFormMetric.NewTaxFormMetric("code-252", "label-795");
        var view = TaxFormMetricView.ToTaxFormMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormMetricView.ToTaxFormMetricViews(new List<TaxFormMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormMetricViewShowsTheStatusInCapitals();
    }
}
