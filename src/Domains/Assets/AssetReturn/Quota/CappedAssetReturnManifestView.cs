namespace Acme.Workforce.Assets.AssetReturn.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped asset return manifest.</summary>
public sealed record CappedAssetReturnManifestView(string Code, string TitleText, string VisibleText, string Region, string Notes, string Hours, string StatusLabel)
{
    public static CappedAssetReturnManifestView ToCappedAssetReturnManifestView(CappedAssetReturnManifest item)
    {
        return new CappedAssetReturnManifestView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            Text.Truncate(item.Region, 64),
            item.Notes,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAssetReturnManifestView> ToCappedAssetReturnManifestViews(List<CappedAssetReturnManifest> items)
    {
        var views = new List<CappedAssetReturnManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAssetReturnManifestView(item));
        }
        return views;
    }

    public static double SumCappedAssetReturnManifestView(CappedAssetReturnManifestView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
