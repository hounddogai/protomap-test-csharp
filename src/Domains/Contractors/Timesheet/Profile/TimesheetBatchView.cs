namespace Acme.Workforce.Contractors.Timesheet.Profile;

using System.Globalization;

/// <summary>How screens and exports show a timesheet batch.</summary>
public sealed record TimesheetBatchView(string Code, string Name, string Cost, string Approved, string ThresholdText, string Capacity, string StatusLabel)
{
    public static TimesheetBatchView ToTimesheetBatchView(TimesheetBatch item)
    {
        return new TimesheetBatchView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetBatchView> ToTimesheetBatchViews(List<TimesheetBatch> items)
    {
        var views = new List<TimesheetBatchView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetBatchView(item));
        }
        return views;
    }

    public static double SumTimesheetBatchView(TimesheetBatchView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedCost + parsedThreshold + (double)parsedCapacity;
    }
}
