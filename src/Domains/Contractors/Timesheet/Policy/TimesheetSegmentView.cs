namespace Acme.Workforce.Contractors.Timesheet.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet segment.</summary>
public sealed record TimesheetSegmentView(string Code, string NameText, string Revision, string Priority, string Skill, string SummaryText, string StatusLabel)
{
    public static TimesheetSegmentView ToTimesheetSegmentView(TimesheetSegment item)
    {
        return new TimesheetSegmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetSegmentView> ToTimesheetSegmentViews(List<TimesheetSegment> items)
    {
        var views = new List<TimesheetSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetSegmentView(item));
        }
        return views;
    }

    public static double SumTimesheetSegmentView(TimesheetSegmentView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedPriority;
    }
}
