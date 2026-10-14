namespace Acme.Workforce.Contractors.Timesheet.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy timesheet metric.</summary>
public sealed record LegacyTimesheetMetricView(string CodeText, string Title, string OwnerIdText, string EndsAt, string CreatedAt, string Fte, string Phone, string Segment, string StatusLabel)
{
    public static LegacyTimesheetMetricView ToLegacyTimesheetMetricView(LegacyTimesheetMetric item)
    {
        return new LegacyTimesheetMetricView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.OwnerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Segment, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTimesheetMetricView> ToLegacyTimesheetMetricViews(List<LegacyTimesheetMetric> items)
    {
        var views = new List<LegacyTimesheetMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTimesheetMetricView(item));
        }
        return views;
    }

    public static double SumLegacyTimesheetMetricView(LegacyTimesheetMetricView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedCreatedAt + parsedFte;
    }
}
