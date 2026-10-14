namespace Acme.Workforce.Assets.Warranty.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a batched warranty record.</summary>
public sealed record BatchedWarrantyRecordView(string Code, string Title, string Minimum, string TenureMonths, string Reviewer, string Ceiling, string Archived, string StatusLabel)
{
    public static BatchedWarrantyRecordView ToBatchedWarrantyRecordView(BatchedWarrantyRecord item)
    {
        return new BatchedWarrantyRecordView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedWarrantyRecordView> ToBatchedWarrantyRecordViews(List<BatchedWarrantyRecord> items)
    {
        var views = new List<BatchedWarrantyRecordView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedWarrantyRecordView(item));
        }
        return views;
    }

    public static double SumBatchedWarrantyRecordView(BatchedWarrantyRecordView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedTenureMonths + parsedCeiling;
    }
}
