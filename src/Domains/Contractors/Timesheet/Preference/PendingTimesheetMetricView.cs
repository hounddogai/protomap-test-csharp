namespace Acme.Workforce.Contractors.Timesheet.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending timesheet metric.</summary>
public sealed record PendingTimesheetMetricView(string Code, string Label, string Rating, string Country, string Priority, string JobCode, string DescriptionText, string StatusLabel)
{
    public static PendingTimesheetMetricView ToPendingTimesheetMetricView(PendingTimesheetMetric item)
    {
        return new PendingTimesheetMetricView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTimesheetMetricView> ToPendingTimesheetMetricViews(List<PendingTimesheetMetric> items)
    {
        var views = new List<PendingTimesheetMetricView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTimesheetMetricView(item));
        }
        return views;
    }

    public static double SumPendingTimesheetMetricView(PendingTimesheetMetricView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedPriority;
    }
}
