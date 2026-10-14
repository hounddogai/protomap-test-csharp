namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return override.</summary>
public sealed record AssetReturnOverrideView(string Code, string LabelText, string Grade, string Source, string Quantity, string StatusLabel)
{
    public static AssetReturnOverrideView ToAssetReturnOverrideView(AssetReturnOverride item)
    {
        return new AssetReturnOverrideView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Grade, 120),
            Text.Truncate(item.Source, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnOverrideView> ToAssetReturnOverrideViews(List<AssetReturnOverride> items)
    {
        var views = new List<AssetReturnOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnOverrideView(item));
        }
        return views;
    }

    public static double SumAssetReturnOverrideView(AssetReturnOverrideView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
