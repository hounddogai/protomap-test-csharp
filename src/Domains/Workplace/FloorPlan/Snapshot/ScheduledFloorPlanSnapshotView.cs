namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled floor plan snapshot.</summary>
public sealed record ScheduledFloorPlanSnapshotView(string Code, string TitleText, string Version, string SalaryBand, string WindowDaysText, string Ceiling, string EmployeeIdText, string StatusLabel)
{
    public static ScheduledFloorPlanSnapshotView ToScheduledFloorPlanSnapshotView(ScheduledFloorPlanSnapshot item)
    {
        return new ScheduledFloorPlanSnapshotView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFloorPlanSnapshotView> ToScheduledFloorPlanSnapshotViews(List<ScheduledFloorPlanSnapshot> items)
    {
        var views = new List<ScheduledFloorPlanSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFloorPlanSnapshotView(item));
        }
        return views;
    }

    public static double SumScheduledFloorPlanSnapshotView(ScheduledFloorPlanSnapshotView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedWindowDays + parsedCeiling;
    }
}
