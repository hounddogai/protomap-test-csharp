namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using System.Globalization;

/// <summary>How screens and exports show a manual floor plan metric.</summary>
public sealed record ManualFloorPlanMetricView(string Code, string LabelText, string Weight, string TitleText, string Score, string StatusLabel)
{
    public static ManualFloorPlanMetricView ToManualFloorPlanMetricView(ManualFloorPlanMetric item)
    {
        return new ManualFloorPlanMetricView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualFloorPlanMetricView> ToManualFloorPlanMetricViews(List<ManualFloorPlanMetric> items)
    {
        var views = new List<ManualFloorPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToManualFloorPlanMetricView(item));
        }
        return views;
    }

    public static double SumManualFloorPlanMetricView(ManualFloorPlanMetricView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedWeight + parsedScore;
    }
}
