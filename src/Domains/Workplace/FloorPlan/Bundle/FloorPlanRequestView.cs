namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan request.</summary>
public sealed record FloorPlanRequestView(string Code, string Label, string Ceiling, string Active, string Hours, string Skill, string Team, string StartsAtText, string Ratio, string StatusLabel)
{
    public static FloorPlanRequestView ToFloorPlanRequestView(FloorPlanRequest item)
    {
        return new FloorPlanRequestView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.Team,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanRequestView> ToFloorPlanRequestViews(List<FloorPlanRequest> items)
    {
        var views = new List<FloorPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanRequestView(item));
        }
        return views;
    }

    public static double SumFloorPlanRequestView(FloorPlanRequestView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedHours + (double)parsedStartsAt;
    }
}
