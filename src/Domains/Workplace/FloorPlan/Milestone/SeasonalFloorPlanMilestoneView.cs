namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal floor plan milestone.</summary>
public sealed record SeasonalFloorPlanMilestoneView(string Code, string Label, string Source, string EmployeeId, string ContactEmail, string Score, string StatusLabel)
{
    public static SeasonalFloorPlanMilestoneView ToSeasonalFloorPlanMilestoneView(SeasonalFloorPlanMilestone item)
    {
        return new SeasonalFloorPlanMilestoneView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Source,
            Text.Truncate(item.EmployeeId, 40),
            item.ContactEmail,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFloorPlanMilestoneView> ToSeasonalFloorPlanMilestoneViews(List<SeasonalFloorPlanMilestone> items)
    {
        var views = new List<SeasonalFloorPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFloorPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumSeasonalFloorPlanMilestoneView(SeasonalFloorPlanMilestoneView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
