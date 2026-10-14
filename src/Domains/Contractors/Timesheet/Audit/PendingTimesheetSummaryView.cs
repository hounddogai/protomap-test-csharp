namespace Acme.Workforce.Contractors.Timesheet.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending timesheet summary.</summary>
public sealed record PendingTimesheetSummaryView(string Code, string Title, string Checksum, string BadgeText, string Category, string Reviewer, string Phone, string AmountText, string StatusLabel)
{
    public static PendingTimesheetSummaryView ToPendingTimesheetSummaryView(PendingTimesheetSummary item)
    {
        return new PendingTimesheetSummaryView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 40),
            item.Checksum,
            Text.Truncate(item.Badge, 40),
            Text.Truncate(item.Category, 80),
            item.Reviewer,
            item.Phone,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTimesheetSummaryView> ToPendingTimesheetSummaryViews(List<PendingTimesheetSummary> items)
    {
        var views = new List<PendingTimesheetSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTimesheetSummaryView(item));
        }
        return views;
    }

    public static double SumPendingTimesheetSummaryView(PendingTimesheetSummaryView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
