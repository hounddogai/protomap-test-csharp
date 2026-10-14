namespace Acme.Workforce.Assets.Laptop.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered laptop baseline.</summary>
public sealed record TieredLaptopBaselineView(string Code, string Title, string ExpiresAtText, string UnitText, string CostText, string Summary, string StatusLabel)
{
    public static TieredLaptopBaselineView ToTieredLaptopBaselineView(TieredLaptopBaseline item)
    {
        return new TieredLaptopBaselineView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredLaptopBaselineView> ToTieredLaptopBaselineViews(List<TieredLaptopBaseline> items)
    {
        var views = new List<TieredLaptopBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToTieredLaptopBaselineView(item));
        }
        return views;
    }

    public static double SumTieredLaptopBaselineView(TieredLaptopBaselineView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedCost;
    }
}
