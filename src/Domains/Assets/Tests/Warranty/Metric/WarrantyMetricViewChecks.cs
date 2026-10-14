namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyMetricViewChecks
{
    public static void WarrantyMetricViewShowsTheStatusInCapitals()
    {
        var item = WarrantyMetric.NewWarrantyMetric("code-625", "title-11");
        var view = WarrantyMetricView.ToWarrantyMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyMetricView.ToWarrantyMetricViews(new List<WarrantyMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyMetricViewShowsTheStatusInCapitals();
    }
}
