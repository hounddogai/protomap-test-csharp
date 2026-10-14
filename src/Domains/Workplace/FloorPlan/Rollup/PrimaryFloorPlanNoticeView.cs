namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary floor plan notice.</summary>
public sealed record PrimaryFloorPlanNoticeView(string CodeText, string Label, string Pinned, string ActiveText, string PriorityText, string Approved, string StatusLabel)
{
    public static PrimaryFloorPlanNoticeView ToPrimaryFloorPlanNoticeView(PrimaryFloorPlanNotice item)
    {
        return new PrimaryFloorPlanNoticeView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            $"{item.Pinned}",
            $"{item.Active}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryFloorPlanNoticeView> ToPrimaryFloorPlanNoticeViews(List<PrimaryFloorPlanNotice> items)
    {
        var views = new List<PrimaryFloorPlanNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryFloorPlanNoticeView(item));
        }
        return views;
    }

    public static double SumPrimaryFloorPlanNoticeView(PrimaryFloorPlanNoticeView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
