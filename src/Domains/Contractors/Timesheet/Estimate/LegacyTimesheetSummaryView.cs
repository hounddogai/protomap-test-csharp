namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy timesheet summary.</summary>
public sealed record LegacyTimesheetSummaryView(string Code, string LabelText, string Priority, string CostCenter, string BadgeText, string Rate, string Notes, string Phone, string CountryText, string ManagerId, string StatusLabel)
{
    public static LegacyTimesheetSummaryView ToLegacyTimesheetSummaryView(LegacyTimesheetSummary item)
    {
        return new LegacyTimesheetSummaryView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Badge, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            Text.Truncate(item.Phone, 80),
            item.Country,
            Text.Truncate(item.ManagerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTimesheetSummaryView> ToLegacyTimesheetSummaryViews(List<LegacyTimesheetSummary> items)
    {
        var views = new List<LegacyTimesheetSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTimesheetSummaryView(item));
        }
        return views;
    }

    public static double SumLegacyTimesheetSummaryView(LegacyTimesheetSummaryView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRate;
    }
}
