namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using System.Globalization;

/// <summary>How screens and exports show a timesheet forecast.</summary>
public sealed record TimesheetForecastView(string Code, string Name, string CapacityText, string Fte, string TenureMonths, string GraceDaysText, string EmployeeId, string Weight, string LeaveDays, string Cost, string StatusLabel)
{
    public static TimesheetForecastView ToTimesheetForecastView(TimesheetForecast item)
    {
        return new TimesheetForecastView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetForecastView> ToTimesheetForecastViews(List<TimesheetForecast> items)
    {
        var views = new List<TimesheetForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetForecastView(item));
        }
        return views;
    }

    public static double SumTimesheetForecastView(TimesheetForecastView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFte + (double)parsedTenureMonths;
    }
}
