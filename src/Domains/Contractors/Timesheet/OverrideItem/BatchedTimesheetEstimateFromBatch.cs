namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Functions of batched timesheet estimate from batch.</summary>
public static class BatchedTimesheetEstimateFromBatch
{
    public static Dictionary<string, double> SummarizeScheduledOffboardingBatchesForEstimate(List<ScheduledOffboardingBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<BatchedTimesheetEstimate> DeriveBatchedTimesheetEstimatesFromBatch(List<ScheduledOffboardingBatch> items)
    {
        var outValue = new List<BatchedTimesheetEstimate>();
        foreach (var item in items)
        {
            var derived = BatchedTimesheetEstimate.NewBatchedTimesheetEstimate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignBatchedTimesheetEstimate(BatchedTimesheetEstimate item)
    {
        return OffboardingRulePolicy.OffboardingRuleBucket(item.MeasuredValue());
    }
}
