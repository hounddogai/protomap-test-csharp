namespace Acme.Workforce.Assets.AssetReturn.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return manifest.</summary>
public sealed record AssetReturnManifestView(string CodeText, string LabelText, string Approved, string ThresholdText, string Timezone, string LocationText, string StatusLabel)
{
    public static AssetReturnManifestView ToAssetReturnManifestView(AssetReturnManifest item)
    {
        return new AssetReturnManifestView(
            Text.Truncate(item.Code, 40),
            item.Label,
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnManifestView> ToAssetReturnManifestViews(List<AssetReturnManifest> items)
    {
        var views = new List<AssetReturnManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnManifestView(item));
        }
        return views;
    }

    public static double SumAssetReturnManifestView(AssetReturnManifestView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
