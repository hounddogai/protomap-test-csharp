namespace Acme.Workforce.Assets.AssetReturn.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return baseline.</summary>
public sealed record AssetReturnBaselineView(string CodeText, string Title, string Department, string Version, string ApprovedText, string Quantity, string SalaryBand, string StartsAt, string Minimum, string StatusLabel)
{
    public static AssetReturnBaselineView ToAssetReturnBaselineView(AssetReturnBaseline item)
    {
        return new AssetReturnBaselineView(
            item.Code,
            item.Title,
            Text.Truncate(item.Department, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnBaselineView> ToAssetReturnBaselineViews(List<AssetReturnBaseline> items)
    {
        var views = new List<AssetReturnBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnBaselineView(item));
        }
        return views;
    }

    public static double SumAssetReturnBaselineView(AssetReturnBaselineView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedQuantity + (double)parsedStartsAt;
    }
}
