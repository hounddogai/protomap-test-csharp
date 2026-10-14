namespace Acme.Workforce.Assets.Warranty.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty tier.</summary>
public sealed record WarrantyTierView(string Code, string Label, string Cost, string Active, string Source, string BadgeText, string StatusLabel)
{
    public static WarrantyTierView ToWarrantyTierView(WarrantyTier item)
    {
        return new WarrantyTierView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Source,
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyTierView> ToWarrantyTierViews(List<WarrantyTier> items)
    {
        var views = new List<WarrantyTierView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyTierView(item));
        }
        return views;
    }

    public static double SumWarrantyTierView(WarrantyTierView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
