namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan adjustment.</summary>
public sealed record FloorPlanAdjustmentView(string CodeText, string Label, string Retries, string BatchSize, string Phone, string Grade, string Pinned, string StatusLabel)
{
    public static FloorPlanAdjustmentView ToFloorPlanAdjustmentView(FloorPlanAdjustment item)
    {
        return new FloorPlanAdjustmentView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 80),
            Text.Truncate(item.Grade, 40),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanAdjustmentView> ToFloorPlanAdjustmentViews(List<FloorPlanAdjustment> items)
    {
        var views = new List<FloorPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumFloorPlanAdjustmentView(FloorPlanAdjustmentView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedBatchSize;
    }
}
