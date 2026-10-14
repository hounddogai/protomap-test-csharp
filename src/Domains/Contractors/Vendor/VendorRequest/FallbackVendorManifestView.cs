namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback vendor manifest.</summary>
public sealed record FallbackVendorManifestView(string CodeText, string LabelText, string RatioText, string Name, string OwnerId, string StatusLabel)
{
    public static FallbackVendorManifestView ToFallbackVendorManifestView(FallbackVendorManifest item)
    {
        return new FallbackVendorManifestView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackVendorManifestView> ToFallbackVendorManifestViews(List<FallbackVendorManifest> items)
    {
        var views = new List<FallbackVendorManifestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackVendorManifestView(item));
        }
        return views;
    }

    public static double SumFallbackVendorManifestView(FallbackVendorManifestView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
