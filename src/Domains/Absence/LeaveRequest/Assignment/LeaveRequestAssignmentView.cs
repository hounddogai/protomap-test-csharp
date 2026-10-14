namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a leave request assignment.</summary>
public sealed record LeaveRequestAssignmentView(string Code, string Title, string EndsAt, string Revision, string LeaveDays, string Rating, string StatusLabel)
{
    public static LeaveRequestAssignmentView ToLeaveRequestAssignmentView(LeaveRequestAssignment item)
    {
        return new LeaveRequestAssignmentView(
            item.Code,
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestAssignmentView> ToLeaveRequestAssignmentViews(List<LeaveRequestAssignment> items)
    {
        var views = new List<LeaveRequestAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestAssignmentView(item));
        }
        return views;
    }

    public static double SumLeaveRequestAssignmentView(LeaveRequestAssignmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRevision + (double)parsedLeaveDays;
    }
}
