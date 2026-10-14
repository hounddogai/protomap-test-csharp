namespace Acme.Workforce.Contractors.Timesheet.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default timesheet preference.</summary>
public sealed record DefaultTimesheetPreferenceView(string Code, string Title, string Pinned, string SegmentText, string Name, string Score, string StatusLabel)
{
    public static DefaultTimesheetPreferenceView ToDefaultTimesheetPreferenceView(DefaultTimesheetPreference item)
    {
        return new DefaultTimesheetPreferenceView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            Text.Truncate(item.Segment, 64),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultTimesheetPreferenceView> ToDefaultTimesheetPreferenceViews(List<DefaultTimesheetPreference> items)
    {
        var views = new List<DefaultTimesheetPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultTimesheetPreferenceView(item));
        }
        return views;
    }

    public static double SumDefaultTimesheetPreferenceView(DefaultTimesheetPreferenceView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
