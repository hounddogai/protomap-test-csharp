namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request snapshot.</summary>
public sealed record LeaveRequestSnapshotView(string CodeText, string LabelText, string Channel, string Version, string Notes, string WindowDaysText, string StatusLabel)
{
    public static LeaveRequestSnapshotView ToLeaveRequestSnapshotView(LeaveRequestSnapshot item)
    {
        return new LeaveRequestSnapshotView(
            item.Code,
            item.Label,
            item.Channel,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestSnapshotView> ToLeaveRequestSnapshotViews(List<LeaveRequestSnapshot> items)
    {
        var views = new List<LeaveRequestSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestSnapshotView(item));
        }
        return views;
    }

    public static double SumLeaveRequestSnapshotView(LeaveRequestSnapshotView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedWindowDays;
    }
}
