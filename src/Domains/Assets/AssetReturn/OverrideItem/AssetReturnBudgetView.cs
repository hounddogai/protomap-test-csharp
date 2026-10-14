namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return budget.</summary>
public sealed record AssetReturnBudgetView(string Code, string Title, string Ceiling, string Active, string Maximum, string ExternalRef, string Team, string StatusLabel)
{
    public static AssetReturnBudgetView ToAssetReturnBudgetView(AssetReturnBudget item)
    {
        return new AssetReturnBudgetView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            Text.Truncate(item.Team, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnBudgetView> ToAssetReturnBudgetViews(List<AssetReturnBudget> items)
    {
        var views = new List<AssetReturnBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnBudgetView(item));
        }
        return views;
    }

    public static double SumAssetReturnBudgetView(AssetReturnBudgetView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMaximum;
    }
}
