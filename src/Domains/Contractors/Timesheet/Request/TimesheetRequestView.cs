namespace Acme.Workforce.Contractors.Timesheet.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet request.</summary>
public sealed record TimesheetRequestView(string CodeText, string Name, string WindowDays, string Source, string SalaryBand, string PhoneText, string Hours, string Minimum, string ContactEmailText, string Priority, string ExternalRefText, string StatusLabel)
{
    public static TimesheetRequestView ToTimesheetRequestView(TimesheetRequest item)
    {
        return new TimesheetRequestView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.SalaryBand,
            item.Phone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetRequestView> ToTimesheetRequestViews(List<TimesheetRequest> items)
    {
        var views = new List<TimesheetRequestView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetRequestView(item));
        }
        return views;
    }

    public static double SumTimesheetRequestView(TimesheetRequestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedHours + parsedMinimum;
    }
}
