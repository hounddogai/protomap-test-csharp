namespace Acme.Workforce.Assets.Warranty.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict warranty tier.</summary>
public sealed record StrictWarrantyTierView(string CodeText, string Name, string Summary, string Margin, string RateText, string Department, string CreatedAt, string Amount, string OwnerId, string Location, string StatusLabel)
{
    public static StrictWarrantyTierView ToStrictWarrantyTierView(StrictWarrantyTier item)
    {
        return new StrictWarrantyTierView(
            item.Code,
            item.Name,
            item.Summary,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictWarrantyTierView> ToStrictWarrantyTierViews(List<StrictWarrantyTier> items)
    {
        var views = new List<StrictWarrantyTierView>();
        foreach (var item in items)
        {
            views.Add(ToStrictWarrantyTierView(item));
        }
        return views;
    }

    public static double SumStrictWarrantyTierView(StrictWarrantyTierView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedMargin + parsedRate + (double)parsedCreatedAt;
    }
}
