namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending timesheet policy.</summary>
public sealed record PendingTimesheetPolicyView(string CodeText, string Title, string Category, string Checksum, string DurationMinutes, string UnitText, string GraceDays, string LeaveDays, string Currency, string StatusLabel)
{
    public static PendingTimesheetPolicyView ToPendingTimesheetPolicyView(PendingTimesheetPolicy item)
    {
        return new PendingTimesheetPolicyView(
            item.Code,
            item.Title,
            item.Category,
            Text.Truncate(item.Checksum, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTimesheetPolicyView> ToPendingTimesheetPolicyViews(List<PendingTimesheetPolicy> items)
    {
        var views = new List<PendingTimesheetPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTimesheetPolicyView(item));
        }
        return views;
    }

    public static double SumPendingTimesheetPolicyView(PendingTimesheetPolicyView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays + (double)parsedLeaveDays;
    }
}
