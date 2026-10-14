namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global asset return profile.</summary>
public sealed record GlobalAssetReturnProfileView(string Code, string LabelText, string CreatedAtText, string Quantity, string Team, string Timezone, string StatusLabel)
{
    public static GlobalAssetReturnProfileView ToGlobalAssetReturnProfileView(GlobalAssetReturnProfile item)
    {
        return new GlobalAssetReturnProfileView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAssetReturnProfileView> ToGlobalAssetReturnProfileViews(List<GlobalAssetReturnProfile> items)
    {
        var views = new List<GlobalAssetReturnProfileView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAssetReturnProfileView(item));
        }
        return views;
    }

    public static double SumGlobalAssetReturnProfileView(GlobalAssetReturnProfileView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedQuantity;
    }
}
