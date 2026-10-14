namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default floor plan segment.</summary>
public sealed record DefaultFloorPlanSegmentView(string Code, string Name, string Score, string LeaveDaysText, string ManagerIdText, string Summary, string Amount, string Visible, string Minimum, string RatioText, string ContactEmail, string StatusLabel)
{
    public static DefaultFloorPlanSegmentView ToDefaultFloorPlanSegmentView(DefaultFloorPlanSegment item)
    {
        return new DefaultFloorPlanSegmentView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.Summary,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultFloorPlanSegmentView> ToDefaultFloorPlanSegmentViews(List<DefaultFloorPlanSegment> items)
    {
        var views = new List<DefaultFloorPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultFloorPlanSegmentView(item));
        }
        return views;
    }

    public static double SumDefaultFloorPlanSegmentView(DefaultFloorPlanSegmentView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + parsedAmount;
    }
}
