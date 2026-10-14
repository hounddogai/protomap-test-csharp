namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Functions of batched leave request forecast from allocation.</summary>
public static class BatchedLeaveRequestForecastFromAllocation
{
    public static Dictionary<string, double> SummarizePolicyAllocationsForForecast(List<PolicyAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<BatchedLeaveRequestForecast> DeriveBatchedLeaveRequestForecastsFromAllocation(List<PolicyAllocation> items)
    {
        var outValue = new List<BatchedLeaveRequestForecast>();
        foreach (var item in items)
        {
            var derived = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeBatchedLeaveRequestForecast(BatchedLeaveRequestForecast item)
    {
        return LenientPolicyThresholdPolicy.LenientPolicyThresholdWeight(8, true);
    }
}
