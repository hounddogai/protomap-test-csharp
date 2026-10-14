namespace Acme.Workforce.Contractors.Timesheet.Summary;

using System.Globalization;

/// <summary>How screens and exports show a timesheet baseline.</summary>
public sealed record TimesheetBaselineView(string Code, string TitleText, string MarginText, string BatchSize, string NameText, string Unit, string EndsAt, string StatusLabel)
{
    public static TimesheetBaselineView ToTimesheetBaselineView(TimesheetBaseline item)
    {
        return new TimesheetBaselineView(
            item.Code,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Unit,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetBaselineView> ToTimesheetBaselineViews(List<TimesheetBaseline> items)
    {
        var views = new List<TimesheetBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetBaselineView(item));
        }
        return views;
    }

    public static double SumTimesheetBaselineView(TimesheetBaselineView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedBatchSize + (double)parsedEndsAt;
    }
}
