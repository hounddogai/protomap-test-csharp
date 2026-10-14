namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy timesheet profile.</summary>
public sealed record LegacyTimesheetProfileView(string Code, string Title, string Category, string Version, string Revision, string StatusLabel)
{
    public static LegacyTimesheetProfileView ToLegacyTimesheetProfileView(LegacyTimesheetProfile item)
    {
        return new LegacyTimesheetProfileView(
            item.Code,
            item.Title,
            Text.Truncate(item.Category, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTimesheetProfileView> ToLegacyTimesheetProfileViews(List<LegacyTimesheetProfile> items)
    {
        var views = new List<LegacyTimesheetProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTimesheetProfileView(item));
        }
        return views;
    }

    public static double SumLegacyTimesheetProfileView(LegacyTimesheetProfileView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedRevision;
    }
}
