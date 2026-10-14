namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class PrimaryVendorMetricViewChecks
{
    public static void PrimaryVendorMetricViewShowsTheStatusInCapitals()
    {
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("code-62", "label-340");
        var view = PrimaryVendorMetricView.ToPrimaryVendorMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryVendorMetricView.ToPrimaryVendorMetricViews(new List<PrimaryVendorMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryVendorMetricViewShowsTheStatusInCapitals();
    }
}
