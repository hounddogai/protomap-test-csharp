namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal warranty tier.</summary>
public sealed record SeasonalWarrantyTierView(string Code, string Name, string Department, string Summary, string VisibleText, string Attempts, string ManagerIdText, string SegmentText, string Margin, string StatusLabel)
{
    public static SeasonalWarrantyTierView ToSeasonalWarrantyTierView(SeasonalWarrantyTier item)
    {
        return new SeasonalWarrantyTierView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Department,
            item.Summary,
            $"{item.Visible}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Segment, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalWarrantyTierView> ToSeasonalWarrantyTierViews(List<SeasonalWarrantyTier> items)
    {
        var views = new List<SeasonalWarrantyTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalWarrantyTierView(item));
        }
        return views;
    }

    public static double SumSeasonalWarrantyTierView(SeasonalWarrantyTierView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedMargin;
    }
}
