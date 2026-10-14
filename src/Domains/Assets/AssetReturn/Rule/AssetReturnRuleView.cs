namespace Acme.Workforce.Assets.AssetReturn.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return rule.</summary>
public sealed record AssetReturnRuleView(string Code, string LabelText, string Ceiling, string GraceDaysText, string Department, string FteText, string Country, string RegionText, string StatusLabel)
{
    public static AssetReturnRuleView ToAssetReturnRuleView(AssetReturnRule item)
    {
        return new AssetReturnRuleView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            Text.Truncate(item.Region, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnRuleView> ToAssetReturnRuleViews(List<AssetReturnRule> items)
    {
        var views = new List<AssetReturnRuleView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnRuleView(item));
        }
        return views;
    }

    public static double SumAssetReturnRuleView(AssetReturnRuleView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedGraceDays + parsedFte;
    }
}
