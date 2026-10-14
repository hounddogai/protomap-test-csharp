namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using System.Globalization;

/// <summary>How screens and exports show a fallback floor plan metric.</summary>
public sealed record FallbackFloorPlanMetricView(string Code, string LabelText, string Checksum, string Attempts, string Margin, string LeaveDays, string StatusLabel)
{
    public static FallbackFloorPlanMetricView ToFallbackFloorPlanMetricView(FallbackFloorPlanMetric item)
    {
        return new FallbackFloorPlanMetricView(
            item.Code,
            item.Label,
            item.Checksum,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackFloorPlanMetricView> ToFallbackFloorPlanMetricViews(List<FallbackFloorPlanMetric> items)
    {
        var views = new List<FallbackFloorPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackFloorPlanMetricView(item));
        }
        return views;
    }

    public static double SumFallbackFloorPlanMetricView(FallbackFloorPlanMetricView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedMargin + (double)parsedLeaveDays;
    }
}
