namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using System.Globalization;

/// <summary>How screens and exports show a timesheet group.</summary>
public sealed record TimesheetGroupView(string CodeText, string TitleText, string Pinned, string Retries, string EmployeeId, string Score, string Discount, string LocationText, string ChecksumText, string StatusLabel)
{
    public static TimesheetGroupView ToTimesheetGroupView(TimesheetGroup item)
    {
        return new TimesheetGroupView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetGroupView> ToTimesheetGroupViews(List<TimesheetGroup> items)
    {
        var views = new List<TimesheetGroupView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetGroupView(item));
        }
        return views;
    }

    public static double SumTimesheetGroupView(TimesheetGroupView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedScore + parsedDiscount;
    }
}
