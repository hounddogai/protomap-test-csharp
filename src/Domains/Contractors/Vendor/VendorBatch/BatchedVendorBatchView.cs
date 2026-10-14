namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched vendor batch.</summary>
public sealed record BatchedVendorBatchView(string Code, string LabelText, string Weight, string LeaveDays, string Score, string Badge, string Grade, string Checksum, string StatusLabel)
{
    public static BatchedVendorBatchView ToBatchedVendorBatchView(BatchedVendorBatch item)
    {
        return new BatchedVendorBatchView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Grade,
            Text.Truncate(item.Checksum, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedVendorBatchView> ToBatchedVendorBatchViews(List<BatchedVendorBatch> items)
    {
        var views = new List<BatchedVendorBatchView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedVendorBatchView(item));
        }
        return views;
    }

    public static double SumBatchedVendorBatchView(BatchedVendorBatchView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedLeaveDays + parsedScore;
    }
}
