namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional leave request item.</summary>
public sealed record RegionalLeaveRequestItemView(string Code, string Label, string LeaveDays, string Location, string BatchSize, string DiscountText, string Summary, string Sequence, string JobCodeText, string ContactEmail, string Priority, string StatusLabel)
{
    public static RegionalLeaveRequestItemView ToRegionalLeaveRequestItemView(RegionalLeaveRequestItem item)
    {
        return new RegionalLeaveRequestItemView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.ContactEmail,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalLeaveRequestItemView> ToRegionalLeaveRequestItemViews(List<RegionalLeaveRequestItem> items)
    {
        var views = new List<RegionalLeaveRequestItemView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalLeaveRequestItemView(item));
        }
        return views;
    }

    public static double SumRegionalLeaveRequestItemView(RegionalLeaveRequestItemView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize + parsedDiscount;
    }
}
