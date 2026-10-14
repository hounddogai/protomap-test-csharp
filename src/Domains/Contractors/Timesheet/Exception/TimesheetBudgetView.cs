namespace Acme.Workforce.Contractors.Timesheet.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet budget.</summary>
public sealed record TimesheetBudgetView(string CodeText, string Name, string Country, string Segment, string SourceText, string Score, string StatusLabel)
{
    public static TimesheetBudgetView ToTimesheetBudgetView(TimesheetBudget item)
    {
        return new TimesheetBudgetView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 40),
            Text.Truncate(item.Segment, 64),
            item.Source,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetBudgetView> ToTimesheetBudgetViews(List<TimesheetBudget> items)
    {
        var views = new List<TimesheetBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetBudgetView(item));
        }
        return views;
    }

    public static double SumTimesheetBudgetView(TimesheetBudgetView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
