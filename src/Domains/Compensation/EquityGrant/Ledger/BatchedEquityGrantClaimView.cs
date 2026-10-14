namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched equity grant claim.</summary>
public sealed record BatchedEquityGrantClaimView(string Code, string Title, string OwnerId, string Category, string FloorValue, string Maximum, string BadgeText, string GraceDaysText, string Capacity, string LeaveDays, string EmployeeIdText, string StatusLabel)
{
    public static BatchedEquityGrantClaimView ToBatchedEquityGrantClaimView(BatchedEquityGrantClaim item)
    {
        return new BatchedEquityGrantClaimView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.OwnerId,
            Text.Truncate(item.Category, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedEquityGrantClaimView> ToBatchedEquityGrantClaimViews(List<BatchedEquityGrantClaim> items)
    {
        var views = new List<BatchedEquityGrantClaimView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedEquityGrantClaimView(item));
        }
        return views;
    }

    public static double SumBatchedEquityGrantClaimView(BatchedEquityGrantClaimView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMaximum + (double)parsedGraceDays;
    }
}
