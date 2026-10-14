namespace Acme.Workforce.Contractors.Timesheet.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet summary.</summary>
public sealed record TimesheetSummaryView(string CodeText, string NameText, string GraceDays, string Phone, string CeilingText, string Visible, string DurationMinutes, string RatioText, string Grade, string OwnerIdText, string StatusLabel)
{
    public static TimesheetSummaryView ToTimesheetSummaryView(TimesheetSummary item)
    {
        return new TimesheetSummaryView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetSummaryView> ToTimesheetSummaryViews(List<TimesheetSummary> items)
    {
        var views = new List<TimesheetSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetSummaryView(item));
        }
        return views;
    }

    public static double SumTimesheetSummaryView(TimesheetSummaryView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedCeiling + (double)parsedDurationMinutes;
    }
}
