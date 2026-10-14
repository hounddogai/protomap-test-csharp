namespace Acme.Workforce.Contractors.Timesheet.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive timesheet manifest.</summary>
public sealed record AdaptiveTimesheetManifestView(string Code, string Label, string Summary, string TenureMonths, string Ceiling, string ActiveText, string CreatedAt, string Badge, string StatusLabel)
{
    public static AdaptiveTimesheetManifestView ToAdaptiveTimesheetManifestView(AdaptiveTimesheetManifest item)
    {
        return new AdaptiveTimesheetManifestView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.Summary, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTimesheetManifestView> ToAdaptiveTimesheetManifestViews(List<AdaptiveTimesheetManifest> items)
    {
        var views = new List<AdaptiveTimesheetManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTimesheetManifestView(item));
        }
        return views;
    }

    public static double SumAdaptiveTimesheetManifestView(AdaptiveTimesheetManifestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedCeiling + (double)parsedCreatedAt;
    }
}
