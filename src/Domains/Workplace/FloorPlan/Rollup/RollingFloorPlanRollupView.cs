namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling floor plan rollup.</summary>
public sealed record RollingFloorPlanRollupView(string Code, string NameText, string MarginText, string Maximum, string FloorValue, string Rating, string Limit, string StatusLabel)
{
    public static RollingFloorPlanRollupView ToRollingFloorPlanRollupView(RollingFloorPlanRollup item)
    {
        return new RollingFloorPlanRollupView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingFloorPlanRollupView> ToRollingFloorPlanRollupViews(List<RollingFloorPlanRollup> items)
    {
        var views = new List<RollingFloorPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToRollingFloorPlanRollupView(item));
        }
        return views;
    }

    public static double SumRollingFloorPlanRollupView(RollingFloorPlanRollupView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedMargin + parsedMaximum + parsedFloorValue;
    }
}
