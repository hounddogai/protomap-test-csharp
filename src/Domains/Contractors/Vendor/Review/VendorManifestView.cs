namespace Acme.Workforce.Contractors.Vendor.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor manifest.</summary>
public sealed record VendorManifestView(string Code, string Label, string Archived, string Channel, string Margin, string Ratio, string ThresholdText, string SummaryText, string Name, string PhoneText, string StatusLabel)
{
    public static VendorManifestView ToVendorManifestView(VendorManifest item)
    {
        return new VendorManifestView(
            Text.Truncate(item.Code, 120),
            item.Label,
            $"{item.Archived}",
            Text.Truncate(item.Channel, 40),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.Name, 64),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorManifestView> ToVendorManifestViews(List<VendorManifest> items)
    {
        var views = new List<VendorManifestView>();
        foreach (var item in items)
        {
            views.Add(ToVendorManifestView(item));
        }
        return views;
    }

    public static double SumVendorManifestView(VendorManifestView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedRatio + parsedThreshold;
    }
}
