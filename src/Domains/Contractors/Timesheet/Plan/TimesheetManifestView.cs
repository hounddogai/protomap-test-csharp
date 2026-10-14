namespace Acme.Workforce.Contractors.Timesheet.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet manifest.</summary>
public sealed record TimesheetManifestView(string CodeText, string Title, string Category, string Badge, string MaximumText, string StatusLabel)
{
    public static TimesheetManifestView ToTimesheetManifestView(TimesheetManifest item)
    {
        return new TimesheetManifestView(
            item.Code,
            item.Title,
            item.Category,
            Text.Truncate(item.Badge, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetManifestView> ToTimesheetManifestViews(List<TimesheetManifest> items)
    {
        var views = new List<TimesheetManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetManifestView(item));
        }
        return views;
    }

    public static double SumTimesheetManifestView(TimesheetManifestView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
