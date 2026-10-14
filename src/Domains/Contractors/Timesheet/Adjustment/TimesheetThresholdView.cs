namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet threshold.</summary>
public sealed record TimesheetThresholdView(string Code, string Title, string SourceText, string JobCode, string Priority, string Budget, string MaximumText, string Unit, string Category, string StatusLabel)
{
    public static TimesheetThresholdView ToTimesheetThresholdView(TimesheetThreshold item)
    {
        return new TimesheetThresholdView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Source, 120),
            item.JobCode,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Category, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetThresholdView> ToTimesheetThresholdViews(List<TimesheetThreshold> items)
    {
        var views = new List<TimesheetThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetThresholdView(item));
        }
        return views;
    }

    public static double SumTimesheetThresholdView(TimesheetThresholdView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedBudget + parsedMaximum;
    }
}
