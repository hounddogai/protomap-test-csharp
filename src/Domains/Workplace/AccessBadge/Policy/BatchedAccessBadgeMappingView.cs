namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using System.Globalization;

/// <summary>How screens and exports show a batched access badge mapping.</summary>
public sealed record BatchedAccessBadgeMappingView(string CodeText, string LabelText, string CostCenter, string CategoryText, string BatchSize, string LeaveDays, string StatusLabel)
{
    public static BatchedAccessBadgeMappingView ToBatchedAccessBadgeMappingView(BatchedAccessBadgeMapping item)
    {
        return new BatchedAccessBadgeMappingView(
            item.Code,
            item.Label,
            item.CostCenter,
            item.Category,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAccessBadgeMappingView> ToBatchedAccessBadgeMappingViews(List<BatchedAccessBadgeMapping> items)
    {
        var views = new List<BatchedAccessBadgeMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAccessBadgeMappingView(item));
        }
        return views;
    }

    public static double SumBatchedAccessBadgeMappingView(BatchedAccessBadgeMappingView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedLeaveDays;
    }
}
