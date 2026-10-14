namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional leave request baseline.</summary>
public sealed record RegionalLeaveRequestBaselineView(string Code, string Name, string Priority, string Score, string PinnedText, string SummaryText, string Segment, string StartsAt, string StatusLabel)
{
    public static RegionalLeaveRequestBaselineView ToRegionalLeaveRequestBaselineView(RegionalLeaveRequestBaseline item)
    {
        return new RegionalLeaveRequestBaselineView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Summary,
            item.Segment,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalLeaveRequestBaselineView> ToRegionalLeaveRequestBaselineViews(List<RegionalLeaveRequestBaseline> items)
    {
        var views = new List<RegionalLeaveRequestBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalLeaveRequestBaselineView(item));
        }
        return views;
    }

    public static double SumRegionalLeaveRequestBaselineView(RegionalLeaveRequestBaselineView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedScore + (double)parsedStartsAt;
    }
}
