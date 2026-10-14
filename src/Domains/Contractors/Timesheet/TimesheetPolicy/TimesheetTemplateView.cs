namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet template.</summary>
public sealed record TimesheetTemplateView(string CodeText, string NameText, string EndsAt, string Fte, string SegmentText, string Source, string Skill, string Grade, string StatusLabel)
{
    public static TimesheetTemplateView ToTimesheetTemplateView(TimesheetTemplate item)
    {
        return new TimesheetTemplateView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Source,
            item.Skill,
            Text.Truncate(item.Grade, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetTemplateView> ToTimesheetTemplateViews(List<TimesheetTemplate> items)
    {
        var views = new List<TimesheetTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetTemplateView(item));
        }
        return views;
    }

    public static double SumTimesheetTemplateView(TimesheetTemplateView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedFte;
    }
}
