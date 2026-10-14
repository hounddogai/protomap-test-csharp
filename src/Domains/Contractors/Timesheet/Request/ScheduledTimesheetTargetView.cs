namespace Acme.Workforce.Contractors.Timesheet.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled timesheet target.</summary>
public sealed record ScheduledTimesheetTargetView(string CodeText, string TitleText, string Reviewer, string DepartmentText, string TenureMonths, string StatusLabel)
{
    public static ScheduledTimesheetTargetView ToScheduledTimesheetTargetView(ScheduledTimesheetTarget item)
    {
        return new ScheduledTimesheetTargetView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 120),
            Text.Truncate(item.Department, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTimesheetTargetView> ToScheduledTimesheetTargetViews(List<ScheduledTimesheetTarget> items)
    {
        var views = new List<ScheduledTimesheetTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTimesheetTargetView(item));
        }
        return views;
    }

    public static double SumScheduledTimesheetTargetView(ScheduledTimesheetTargetView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
