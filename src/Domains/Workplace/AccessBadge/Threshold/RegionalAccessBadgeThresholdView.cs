namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional access badge threshold.</summary>
public sealed record RegionalAccessBadgeThresholdView(string CodeText, string Name, string Currency, string UnitText, string Notes, string WeightText, string SegmentText, string DurationMinutes, string Team, string FloorValue, string BatchSize, string StatusLabel)
{
    public static RegionalAccessBadgeThresholdView ToRegionalAccessBadgeThresholdView(RegionalAccessBadgeThreshold item)
    {
        return new RegionalAccessBadgeThresholdView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Currency, 120),
            item.Unit,
            Text.Truncate(item.Notes, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAccessBadgeThresholdView> ToRegionalAccessBadgeThresholdViews(List<RegionalAccessBadgeThreshold> items)
    {
        var views = new List<RegionalAccessBadgeThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAccessBadgeThresholdView(item));
        }
        return views;
    }

    public static double SumRegionalAccessBadgeThresholdView(RegionalAccessBadgeThresholdView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedDurationMinutes + parsedFloorValue;
    }
}
