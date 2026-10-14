namespace Acme.Workforce.Contractors.Timesheet.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default timesheet record.</summary>
public sealed record DefaultTimesheetRecordView(string CodeText, string Name, string SkillText, string MinimumText, string Category, string GraceDays, string Visible, string Notes, string Source, string StatusLabel)
{
    public static DefaultTimesheetRecordView ToDefaultTimesheetRecordView(DefaultTimesheetRecord item)
    {
        return new DefaultTimesheetRecordView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Skill,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Notes,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultTimesheetRecordView> ToDefaultTimesheetRecordViews(List<DefaultTimesheetRecord> items)
    {
        var views = new List<DefaultTimesheetRecordView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultTimesheetRecordView(item));
        }
        return views;
    }

    public static double SumDefaultTimesheetRecordView(DefaultTimesheetRecordView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedGraceDays;
    }
}
