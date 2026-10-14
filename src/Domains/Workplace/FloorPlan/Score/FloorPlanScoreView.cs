namespace Acme.Workforce.Workplace.FloorPlan.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan score.</summary>
public sealed record FloorPlanScoreView(string Code, string Name, string ChecksumText, string CreatedAt, string SalaryBand, string ManagerId, string StatusLabel)
{
    public static FloorPlanScoreView ToFloorPlanScoreView(FloorPlanScore item)
    {
        return new FloorPlanScoreView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Checksum,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanScoreView> ToFloorPlanScoreViews(List<FloorPlanScore> items)
    {
        var views = new List<FloorPlanScoreView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanScoreView(item));
        }
        return views;
    }

    public static double SumFloorPlanScoreView(FloorPlanScoreView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
