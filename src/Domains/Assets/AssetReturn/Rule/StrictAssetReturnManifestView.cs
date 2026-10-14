namespace Acme.Workforce.Assets.AssetReturn.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict asset return manifest.</summary>
public sealed record StrictAssetReturnManifestView(string Code, string Name, string Budget, string Minimum, string Amount, string StatusLabel)
{
    public static StrictAssetReturnManifestView ToStrictAssetReturnManifestView(StrictAssetReturnManifest item)
    {
        return new StrictAssetReturnManifestView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAssetReturnManifestView> ToStrictAssetReturnManifestViews(List<StrictAssetReturnManifest> items)
    {
        var views = new List<StrictAssetReturnManifestView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAssetReturnManifestView(item));
        }
        return views;
    }

    public static double SumStrictAssetReturnManifestView(StrictAssetReturnManifestView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMinimum + parsedAmount;
    }
}
