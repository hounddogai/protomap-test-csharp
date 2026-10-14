namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet digest.</summary>
public sealed record TimesheetDigestView(string Code, string LabelText, string Category, string SegmentText, string TenureMonths, string Budget, string Summary, string Attempts, string StatusLabel)
{
    public static TimesheetDigestView ToTimesheetDigestView(TimesheetDigest item)
    {
        return new TimesheetDigestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 120),
            item.Category,
            item.Segment,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetDigestView> ToTimesheetDigestViews(List<TimesheetDigest> items)
    {
        var views = new List<TimesheetDigestView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetDigestView(item));
        }
        return views;
    }

    public static double SumTimesheetDigestView(TimesheetDigestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedBudget + (double)parsedAttempts;
    }
}
