namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet ledger.</summary>
public sealed record TimesheetLedgerView(string Code, string LabelText, string TeamText, string Version, string Timezone, string Rating, string EmployeeId, string Threshold, string StatusLabel)
{
    public static TimesheetLedgerView ToTimesheetLedgerView(TimesheetLedger item)
    {
        return new TimesheetLedgerView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Team,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetLedgerView> ToTimesheetLedgerViews(List<TimesheetLedger> items)
    {
        var views = new List<TimesheetLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetLedgerView(item));
        }
        return views;
    }

    public static double SumTimesheetLedgerView(TimesheetLedgerView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRating + parsedThreshold;
    }
}
