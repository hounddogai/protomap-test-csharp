namespace Acme.Workforce.Contractors.Timesheet.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet variance.</summary>
public sealed record TimesheetVarianceView(string Code, string Label, string Archived, string ScoreText, string CreatedAtText, string RateText, string LimitText, string Reviewer, string FteText, string StatusLabel)
{
    public static TimesheetVarianceView ToTimesheetVarianceView(TimesheetVariance item)
    {
        return new TimesheetVarianceView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetVarianceView> ToTimesheetVarianceViews(List<TimesheetVariance> items)
    {
        var views = new List<TimesheetVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetVarianceView(item));
        }
        return views;
    }

    public static double SumTimesheetVarianceView(TimesheetVarianceView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedCreatedAt + parsedRate;
    }
}
