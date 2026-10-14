namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a timesheet metric.</summary>
public sealed record TimesheetMetricView(string CodeText, string Name, string ManagerId, string DiscountText, string Visible, string ContactEmail, string Ratio, string StatusLabel)
{
    public static TimesheetMetricView ToTimesheetMetricView(TimesheetMetric item)
    {
        return new TimesheetMetricView(
            item.Code,
            item.Name,
            item.ManagerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.ContactEmail,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetMetricView> ToTimesheetMetricViews(List<TimesheetMetric> items)
    {
        var views = new List<TimesheetMetricView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetMetricView(item));
        }
        return views;
    }

    public static double SumTimesheetMetricView(TimesheetMetricView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedRatio;
    }
}
