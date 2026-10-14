namespace Acme.Workforce.Contractors.Timesheet.Review;

using System.Globalization;

/// <summary>How screens and exports show a lenient timesheet score.</summary>
public sealed record LenientTimesheetScoreView(string CodeText, string LabelText, string Description, string WindowDaysText, string Visible, string Amount, string Score, string UpdatedAt, string Limit, string StatusLabel)
{
    public static LenientTimesheetScoreView ToLenientTimesheetScoreView(LenientTimesheetScore item)
    {
        return new LenientTimesheetScoreView(
            item.Code,
            item.Label,
            item.Description,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientTimesheetScoreView> ToLenientTimesheetScoreViews(List<LenientTimesheetScore> items)
    {
        var views = new List<LenientTimesheetScoreView>();
        foreach (var item in items)
        {
            views.Add(ToLenientTimesheetScoreView(item));
        }
        return views;
    }

    public static double SumLenientTimesheetScoreView(LenientTimesheetScoreView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedAmount + parsedScore;
    }
}
