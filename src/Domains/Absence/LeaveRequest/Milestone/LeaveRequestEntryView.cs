namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request entry.</summary>
public sealed record LeaveRequestEntryView(string CodeText, string Name, string Visible, string FloorValue, string SummaryText, string Priority, string Reviewer, string Rating, string Label, string StatusLabel)
{
    public static LeaveRequestEntryView ToLeaveRequestEntryView(LeaveRequestEntry item)
    {
        return new LeaveRequestEntryView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestEntryView> ToLeaveRequestEntryViews(List<LeaveRequestEntry> items)
    {
        var views = new List<LeaveRequestEntryView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestEntryView(item));
        }
        return views;
    }

    public static double SumLeaveRequestEntryView(LeaveRequestEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedPriority + parsedRating;
    }
}
