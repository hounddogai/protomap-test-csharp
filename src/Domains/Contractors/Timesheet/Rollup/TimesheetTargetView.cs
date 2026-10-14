namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet target.</summary>
public sealed record TimesheetTargetView(string Code, string Label, string SegmentText, string Phone, string LimitText, string Capacity, string RegionText, string CountryText, string Grade, string Hours, string StatusLabel)
{
    public static TimesheetTargetView ToTimesheetTargetView(TimesheetTarget item)
    {
        return new TimesheetTargetView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Segment, 40),
            item.Phone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Country, 80),
            Text.Truncate(item.Grade, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetTargetView> ToTimesheetTargetViews(List<TimesheetTarget> items)
    {
        var views = new List<TimesheetTargetView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetTargetView(item));
        }
        return views;
    }

    public static double SumTimesheetTargetView(TimesheetTargetView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedCapacity + parsedHours;
    }
}
