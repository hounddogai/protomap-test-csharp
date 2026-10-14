namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling floor plan segment.</summary>
public sealed record RollingFloorPlanSegmentView(string CodeText, string NameText, string CurrencyText, string Title, string OwnerIdText, string Ratio, string RatingText, string WindowDaysText, string Label, string EndsAt, string RevisionText, string StatusLabel)
{
    public static RollingFloorPlanSegmentView ToRollingFloorPlanSegmentView(RollingFloorPlanSegment item)
    {
        return new RollingFloorPlanSegmentView(
            item.Code,
            item.Name,
            item.Currency,
            Text.Truncate(item.Title, 40),
            item.OwnerId,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingFloorPlanSegmentView> ToRollingFloorPlanSegmentViews(List<RollingFloorPlanSegment> items)
    {
        var views = new List<RollingFloorPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingFloorPlanSegmentView(item));
        }
        return views;
    }

    public static double SumRollingFloorPlanSegmentView(RollingFloorPlanSegmentView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedRating + (double)parsedWindowDays;
    }
}
