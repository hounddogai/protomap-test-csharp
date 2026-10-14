namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request schedule.</summary>
public sealed record LeaveRequestScheduleView(string Code, string TitleText, string Region, string Checksum, string OwnerId, string Fte, string Revision, string Location, string Minimum, string StatusLabel)
{
    public static LeaveRequestScheduleView ToLeaveRequestScheduleView(LeaveRequestSchedule item)
    {
        return new LeaveRequestScheduleView(
            item.Code,
            item.Title,
            item.Region,
            Text.Truncate(item.Checksum, 120),
            item.OwnerId,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestScheduleView> ToLeaveRequestScheduleViews(List<LeaveRequestSchedule> items)
    {
        var views = new List<LeaveRequestScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestScheduleView(item));
        }
        return views;
    }

    public static double SumLeaveRequestScheduleView(LeaveRequestScheduleView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedRevision + parsedMinimum;
    }
}
