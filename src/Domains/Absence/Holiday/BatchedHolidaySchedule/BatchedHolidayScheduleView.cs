namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched holiday schedule.</summary>
public sealed record BatchedHolidayScheduleView(string Code, string Name, string JobCodeText, string Description, string Cost, string ManagerIdText, string StatusLabel)
{
    public static BatchedHolidayScheduleView ToBatchedHolidayScheduleView(BatchedHolidaySchedule item)
    {
        return new BatchedHolidayScheduleView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.JobCode,
            Text.Truncate(item.Description, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedHolidayScheduleView> ToBatchedHolidayScheduleViews(List<BatchedHolidaySchedule> items)
    {
        var views = new List<BatchedHolidayScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedHolidayScheduleView(item));
        }
        return views;
    }

    public static double SumBatchedHolidayScheduleView(BatchedHolidayScheduleView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
