namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive floor plan adjustment.</summary>
public sealed record AdaptiveFloorPlanAdjustmentView(string Code, string Label, string Name, string DurationMinutes, string Department, string StatusLabel)
{
    public static AdaptiveFloorPlanAdjustmentView ToAdaptiveFloorPlanAdjustmentView(AdaptiveFloorPlanAdjustment item)
    {
        return new AdaptiveFloorPlanAdjustmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Name, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveFloorPlanAdjustmentView> ToAdaptiveFloorPlanAdjustmentViews(List<AdaptiveFloorPlanAdjustment> items)
    {
        var views = new List<AdaptiveFloorPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveFloorPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveFloorPlanAdjustmentView(AdaptiveFloorPlanAdjustmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
