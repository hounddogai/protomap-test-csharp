namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered cost model entry.</summary>
public sealed record TieredCostModelEntryView(string Code, string Name, string RegionText, string TitleText, string Margin, string TimezoneText, string Discount, string Budget, string ExpiresAt, string EndsAt, string StatusLabel)
{
    public static TieredCostModelEntryView ToTieredCostModelEntryView(TieredCostModelEntry item)
    {
        return new TieredCostModelEntryView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Region,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCostModelEntryView> ToTieredCostModelEntryViews(List<TieredCostModelEntry> items)
    {
        var views = new List<TieredCostModelEntryView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCostModelEntryView(item));
        }
        return views;
    }

    public static double SumTieredCostModelEntryView(TieredCostModelEntryView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMargin + parsedDiscount + parsedBudget;
    }
}
