namespace Acme.Workforce.Absence.Holiday.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered holiday snapshot.</summary>
public sealed record TieredHolidaySnapshotView(string Code, string Name, string Active, string Archived, string EmployeeIdText, string Label, string Notes, string EndsAt, string Location, string Score, string StatusLabel)
{
    public static TieredHolidaySnapshotView ToTieredHolidaySnapshotView(TieredHolidaySnapshot item)
    {
        return new TieredHolidaySnapshotView(
            item.Code,
            item.Name,
            $"{item.Active}",
            $"{item.Archived}",
            item.EmployeeId,
            item.Label,
            item.Notes,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredHolidaySnapshotView> ToTieredHolidaySnapshotViews(List<TieredHolidaySnapshot> items)
    {
        var views = new List<TieredHolidaySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToTieredHolidaySnapshotView(item));
        }
        return views;
    }

    public static double SumTieredHolidaySnapshotView(TieredHolidaySnapshotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedScore;
    }
}
