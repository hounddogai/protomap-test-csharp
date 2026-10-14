namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending vendor manifest.</summary>
public sealed record PendingVendorManifestView(string Code, string Label, string PhoneText, string RegionText, string StartsAt, string ExternalRef, string Discount, string StatusLabel)
{
    public static PendingVendorManifestView ToPendingVendorManifestView(PendingVendorManifest item)
    {
        return new PendingVendorManifestView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Phone,
            item.Region,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVendorManifestView> ToPendingVendorManifestViews(List<PendingVendorManifest> items)
    {
        var views = new List<PendingVendorManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVendorManifestView(item));
        }
        return views;
    }

    public static double SumPendingVendorManifestView(PendingVendorManifestView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedDiscount;
    }
}
