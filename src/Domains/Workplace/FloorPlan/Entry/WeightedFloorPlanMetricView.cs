namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted floor plan metric.</summary>
public sealed record WeightedFloorPlanMetricView(string Code, string Label, string Phone, string MarginText, string ExternalRefText, string SalaryBand, string Ceiling, string Channel, string StatusLabel)
{
    public static WeightedFloorPlanMetricView ToWeightedFloorPlanMetricView(WeightedFloorPlanMetric item)
    {
        return new WeightedFloorPlanMetricView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Phone,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.SalaryBand,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedFloorPlanMetricView> ToWeightedFloorPlanMetricViews(List<WeightedFloorPlanMetric> items)
    {
        var views = new List<WeightedFloorPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedFloorPlanMetricView(item));
        }
        return views;
    }

    public static double SumWeightedFloorPlanMetricView(WeightedFloorPlanMetricView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedMargin + parsedCeiling;
    }
}
