namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Functions of batched refresh assignment from request.</summary>
public static class BatchedRefreshAssignmentFromRequest
{
    public static Dictionary<string, double> SummarizeBatchedLaptopRequestsForAssignment(List<BatchedLaptopRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<BatchedRefreshAssignment> DeriveBatchedRefreshAssignmentsFromRequest(List<BatchedLaptopRequest> items)
    {
        var outValue = new List<BatchedRefreshAssignment>();
        foreach (var item in items)
        {
            var derived = BatchedRefreshAssignment.NewBatchedRefreshAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceBatchedRefreshAssignment(BatchedRefreshAssignment item)
    {
        return PrimaryLaptopBatchPolicy.NormalizePrimaryLaptopBatchCode(item.Code);
    }
}
