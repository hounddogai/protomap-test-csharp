namespace Acme.Workforce.Compensation.PayBand.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary pay band manifest.</summary>
public sealed record PrimaryPayBandManifestView(string Code, string TitleText, string ChecksumText, string BadgeText, string PinnedText, string ApprovedText, string Location, string Quantity, string StatusLabel)
{
    public static PrimaryPayBandManifestView ToPrimaryPayBandManifestView(PrimaryPayBandManifest item)
    {
        return new PrimaryPayBandManifestView(
            item.Code,
            item.Title,
            item.Checksum,
            item.Badge,
            $"{item.Pinned}",
            $"{item.Approved}",
            Text.Truncate(item.Location, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPayBandManifestView> ToPrimaryPayBandManifestViews(List<PrimaryPayBandManifest> items)
    {
        var views = new List<PrimaryPayBandManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPayBandManifestView(item));
        }
        return views;
    }

    public static double SumPrimaryPayBandManifestView(PrimaryPayBandManifestView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
