namespace Acme.Workforce.Assets.AssetReturn.Batch;

using System.Globalization;

/// <summary>How screens and exports show a manual asset return batch.</summary>
public sealed record ManualAssetReturnBatchView(string CodeText, string Label, string PriorityText, string CostCenter, string EmployeeIdText, string Rate, string LeaveDaysText, string StatusLabel)
{
    public static ManualAssetReturnBatchView ToManualAssetReturnBatchView(ManualAssetReturnBatch item)
    {
        return new ManualAssetReturnBatchView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.EmployeeId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAssetReturnBatchView> ToManualAssetReturnBatchViews(List<ManualAssetReturnBatch> items)
    {
        var views = new List<ManualAssetReturnBatchView>();
        foreach (var item in items)
        {
            views.Add(ToManualAssetReturnBatchView(item));
        }
        return views;
    }

    public static double SumManualAssetReturnBatchView(ManualAssetReturnBatchView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRate + (double)parsedLeaveDays;
    }
}
