namespace Acme.Workforce.Compliance.Exception.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched exception record.</summary>
public sealed record BatchedExceptionRecordView(string Code, string NameText, string Threshold, string CategoryText, string MarginText, string CostCenter, string StatusLabel)
{
    public static BatchedExceptionRecordView ToBatchedExceptionRecordView(BatchedExceptionRecord item)
    {
        return new BatchedExceptionRecordView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedExceptionRecordView> ToBatchedExceptionRecordViews(List<BatchedExceptionRecord> items)
    {
        var views = new List<BatchedExceptionRecordView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedExceptionRecordView(item));
        }
        return views;
    }

    public static double SumBatchedExceptionRecordView(BatchedExceptionRecordView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMargin;
    }
}
