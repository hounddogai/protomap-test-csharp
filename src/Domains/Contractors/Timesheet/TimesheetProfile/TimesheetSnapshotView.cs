namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet snapshot.</summary>
public sealed record TimesheetSnapshotView(string Code, string TitleText, string Phone, string Sequence, string WindowDaysText, string StatusLabel)
{
    public static TimesheetSnapshotView ToTimesheetSnapshotView(TimesheetSnapshot item)
    {
        return new TimesheetSnapshotView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Phone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetSnapshotView> ToTimesheetSnapshotViews(List<TimesheetSnapshot> items)
    {
        var views = new List<TimesheetSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetSnapshotView(item));
        }
        return views;
    }

    public static double SumTimesheetSnapshotView(TimesheetSnapshotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedWindowDays;
    }
}
