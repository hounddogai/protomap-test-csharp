namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>Functions of batched balance override from template.</summary>
public static class BatchedBalanceOverrideFromTemplate
{
    public static Dictionary<string, double> SummarizeFallbackLeaveRequestTemplatesForOverride(List<FallbackLeaveRequestTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<BatchedBalanceOverride> DeriveBatchedBalanceOverridesFromTemplate(List<FallbackLeaveRequestTemplate> items)
    {
        var outValue = new List<BatchedBalanceOverride>();
        foreach (var item in items)
        {
            var derived = BatchedBalanceOverride.NewBatchedBalanceOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignBatchedBalanceOverride(BatchedBalanceOverride item)
    {
        return BatchedLeaveRequestForecastPolicy.BatchedLeaveRequestForecastWeight(2, true);
    }
}
