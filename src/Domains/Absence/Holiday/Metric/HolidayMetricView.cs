namespace Acme.Workforce.Absence.Holiday.Metric;

using System.Globalization;

/// <summary>How screens and exports show a holiday metric.</summary>
public sealed record HolidayMetricView(string Code, string Name, string Retries, string DepartmentText, string Approved, string GraceDays, string Hours, string StatusLabel)
{
    public static HolidayMetricView ToHolidayMetricView(HolidayMetric item)
    {
        return new HolidayMetricView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Approved}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayMetricView> ToHolidayMetricViews(List<HolidayMetric> items)
    {
        var views = new List<HolidayMetricView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayMetricView(item));
        }
        return views;
    }

    public static double SumHolidayMetricView(HolidayMetricView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedGraceDays + parsedHours;
    }
}
