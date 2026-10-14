namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using System.Globalization;

/// <summary>How screens and exports show a primary vendor metric.</summary>
public sealed record PrimaryVendorMetricView(string Code, string Label, string RevisionText, string Retries, string DepartmentText, string SummaryText, string StatusLabel)
{
    public static PrimaryVendorMetricView ToPrimaryVendorMetricView(PrimaryVendorMetric item)
    {
        return new PrimaryVendorMetricView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryVendorMetricView> ToPrimaryVendorMetricViews(List<PrimaryVendorMetric> items)
    {
        var views = new List<PrimaryVendorMetricView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryVendorMetricView(item));
        }
        return views;
    }

    public static double SumPrimaryVendorMetricView(PrimaryVendorMetricView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedRetries;
    }
}
