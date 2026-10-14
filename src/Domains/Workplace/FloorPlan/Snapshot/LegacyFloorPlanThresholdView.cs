namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy floor plan threshold.</summary>
public sealed record LegacyFloorPlanThresholdView(string Code, string Title, string Minimum, string CategoryText, string QuantityText, string Pinned, string FteText, string Badge, string StatusLabel)
{
    public static LegacyFloorPlanThresholdView ToLegacyFloorPlanThresholdView(LegacyFloorPlanThreshold item)
    {
        return new LegacyFloorPlanThresholdView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyFloorPlanThresholdView> ToLegacyFloorPlanThresholdViews(List<LegacyFloorPlanThreshold> items)
    {
        var views = new List<LegacyFloorPlanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyFloorPlanThresholdView(item));
        }
        return views;
    }

    public static double SumLegacyFloorPlanThresholdView(LegacyFloorPlanThresholdView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedQuantity + parsedFte;
    }
}
