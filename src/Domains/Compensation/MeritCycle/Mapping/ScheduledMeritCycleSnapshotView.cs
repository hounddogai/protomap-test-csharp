namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled merit cycle snapshot.</summary>
public sealed record ScheduledMeritCycleSnapshotView(string CodeText, string Name, string Team, string ChecksumText, string Ceiling, string Timezone, string LeaveDaysText, string WindowDays, string Threshold, string StatusLabel)
{
    public static ScheduledMeritCycleSnapshotView ToScheduledMeritCycleSnapshotView(ScheduledMeritCycleSnapshot item)
    {
        return new ScheduledMeritCycleSnapshotView(
            item.Code,
            item.Name,
            Text.Truncate(item.Team, 120),
            item.Checksum,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledMeritCycleSnapshotView> ToScheduledMeritCycleSnapshotViews(List<ScheduledMeritCycleSnapshot> items)
    {
        var views = new List<ScheduledMeritCycleSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledMeritCycleSnapshotView(item));
        }
        return views;
    }

    public static double SumScheduledMeritCycleSnapshotView(ScheduledMeritCycleSnapshotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedLeaveDays + (double)parsedWindowDays;
    }
}
