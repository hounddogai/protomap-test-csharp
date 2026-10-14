namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan claim.</summary>
public sealed record FloorPlanClaimView(string Code, string Label, string Score, string Name, string Notes, string Pinned, string Skill, string Title, string Cost, string JobCode, string StatusLabel)
{
    public static FloorPlanClaimView ToFloorPlanClaimView(FloorPlanClaim item)
    {
        return new FloorPlanClaimView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Notes,
            $"{item.Pinned}",
            item.Skill,
            Text.Truncate(item.Title, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanClaimView> ToFloorPlanClaimViews(List<FloorPlanClaim> items)
    {
        var views = new List<FloorPlanClaimView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanClaimView(item));
        }
        return views;
    }

    public static double SumFloorPlanClaimView(FloorPlanClaimView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedScore + parsedCost;
    }
}
