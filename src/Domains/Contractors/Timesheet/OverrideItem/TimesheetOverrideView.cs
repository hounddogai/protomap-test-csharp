namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet override.</summary>
public sealed record TimesheetOverrideView(string Code, string Label, string Version, string RegionText, string GraceDays, string ManagerIdText, string NameText, string Notes, string StatusLabel)
{
    public static TimesheetOverrideView ToTimesheetOverrideView(TimesheetOverride item)
    {
        return new TimesheetOverrideView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Name, 120),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetOverrideView> ToTimesheetOverrideViews(List<TimesheetOverride> items)
    {
        var views = new List<TimesheetOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetOverrideView(item));
        }
        return views;
    }

    public static double SumTimesheetOverrideView(TimesheetOverrideView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedGraceDays;
    }
}
