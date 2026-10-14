namespace Acme.Workforce.Compensation.Adjustment.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched adjustment claim.</summary>
public sealed record BatchedAdjustmentClaimView(string CodeText, string Title, string Rate, string Notes, string LocationText, string FloorValue, string Retries, string StatusLabel)
{
    public static BatchedAdjustmentClaimView ToBatchedAdjustmentClaimView(BatchedAdjustmentClaim item)
    {
        return new BatchedAdjustmentClaimView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            item.Location,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAdjustmentClaimView> ToBatchedAdjustmentClaimViews(List<BatchedAdjustmentClaim> items)
    {
        var views = new List<BatchedAdjustmentClaimView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAdjustmentClaimView(item));
        }
        return views;
    }

    public static double SumBatchedAdjustmentClaimView(BatchedAdjustmentClaimView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedRate + parsedFloorValue + (double)parsedRetries;
    }
}
