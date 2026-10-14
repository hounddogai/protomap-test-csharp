namespace Acme.Workforce.Absence.LeaveRequest.Item;

using System.Globalization;

/// <summary>How screens and exports show a leave request item.</summary>
public sealed record LeaveRequestItemView(string CodeText, string Title, string RevisionText, string Source, string VersionText, string LeaveDays, string Ceiling, string Description, string Visible, string RatioText, string DurationMinutes, string StatusLabel)
{
    public static LeaveRequestItemView ToLeaveRequestItemView(LeaveRequestItem item)
    {
        return new LeaveRequestItemView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Visible}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestItemView> ToLeaveRequestItemViews(List<LeaveRequestItem> items)
    {
        var views = new List<LeaveRequestItemView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestItemView(item));
        }
        return views;
    }

    public static double SumLeaveRequestItemView(LeaveRequestItemView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedVersion + (double)parsedLeaveDays;
    }
}
