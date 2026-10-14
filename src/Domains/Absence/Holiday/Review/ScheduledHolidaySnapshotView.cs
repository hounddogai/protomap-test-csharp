namespace Acme.Workforce.Absence.Holiday.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled holiday snapshot.</summary>
public sealed record ScheduledHolidaySnapshotView(string Code, string Label, string Hours, string CountryText, string UpdatedAt, string Priority, string Version, string Region, string Summary, string Description, string Maximum, string StatusLabel)
{
    public static ScheduledHolidaySnapshotView ToScheduledHolidaySnapshotView(ScheduledHolidaySnapshot item)
    {
        return new ScheduledHolidaySnapshotView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            item.Summary,
            Text.Truncate(item.Description, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledHolidaySnapshotView> ToScheduledHolidaySnapshotViews(List<ScheduledHolidaySnapshot> items)
    {
        var views = new List<ScheduledHolidaySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledHolidaySnapshotView(item));
        }
        return views;
    }

    public static double SumScheduledHolidaySnapshotView(ScheduledHolidaySnapshotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
