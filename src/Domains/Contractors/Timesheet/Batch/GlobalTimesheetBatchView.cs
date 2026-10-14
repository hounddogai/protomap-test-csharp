namespace Acme.Workforce.Contractors.Timesheet.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global timesheet batch.</summary>
public sealed record GlobalTimesheetBatchView(string Code, string Title, string Phone, string Rate, string NameText, string Grade, string StatusLabel)
{
    public static GlobalTimesheetBatchView ToGlobalTimesheetBatchView(GlobalTimesheetBatch item)
    {
        return new GlobalTimesheetBatchView(
            item.Code,
            item.Title,
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 80),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalTimesheetBatchView> ToGlobalTimesheetBatchViews(List<GlobalTimesheetBatch> items)
    {
        var views = new List<GlobalTimesheetBatchView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalTimesheetBatchView(item));
        }
        return views;
    }

    public static double SumGlobalTimesheetBatchView(GlobalTimesheetBatchView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
