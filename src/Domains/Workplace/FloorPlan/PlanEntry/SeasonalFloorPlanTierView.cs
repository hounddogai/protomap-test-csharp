namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal floor plan tier.</summary>
public sealed record SeasonalFloorPlanTierView(string Code, string Title, string Weight, string Active, string BatchSize, string Category, string ExternalRefText, string Threshold, string Amount, string StatusLabel)
{
    public static SeasonalFloorPlanTierView ToSeasonalFloorPlanTierView(SeasonalFloorPlanTier item)
    {
        return new SeasonalFloorPlanTierView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            Text.Truncate(item.ExternalRef, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFloorPlanTierView> ToSeasonalFloorPlanTierViews(List<SeasonalFloorPlanTier> items)
    {
        var views = new List<SeasonalFloorPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFloorPlanTierView(item));
        }
        return views;
    }

    public static double SumSeasonalFloorPlanTierView(SeasonalFloorPlanTierView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedBatchSize + parsedThreshold;
    }
}
