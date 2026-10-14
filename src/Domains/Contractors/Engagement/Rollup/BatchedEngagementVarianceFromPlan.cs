namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Functions of batched engagement variance from plan.</summary>
public static class BatchedEngagementVarianceFromPlan
{
    public static Dictionary<string, double> SummarizeFallbackTimesheetPlansForVariance(List<FallbackTimesheetPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<BatchedEngagementVariance> DeriveBatchedEngagementVariancesFromPlan(List<FallbackTimesheetPlan> items)
    {
        var outValue = new List<BatchedEngagementVariance>();
        foreach (var item in items)
        {
            var derived = BatchedEngagementVariance.NewBatchedEngagementVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyBatchedEngagementVariance(BatchedEngagementVariance item)
    {
        return LenientTimesheetPreferencePolicy.ClassifyLenientTimesheetPreference(item.MeasuredValue());
    }
}
