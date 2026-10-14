namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan metric.</summary>
public sealed record FloorPlanMetricView(string Code, string Name, string Ceiling, string Visible, string Title, string RateText, string StatusLabel)
{
    public static FloorPlanMetricView ToFloorPlanMetricView(FloorPlanMetric item)
    {
        return new FloorPlanMetricView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanMetricView> ToFloorPlanMetricViews(List<FloorPlanMetric> items)
    {
        var views = new List<FloorPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanMetricView(item));
        }
        return views;
    }

    public static double SumFloorPlanMetricView(FloorPlanMetricView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedRate;
    }
}
