namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Functions of batched offboarding allocation from preference.</summary>
public static class BatchedOffboardingAllocationFromPreference
{
    public static Dictionary<string, double> SummarizeDefaultTimesheetPreferencesForAllocation(List<DefaultTimesheetPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BatchedOffboardingAllocation> DeriveBatchedOffboardingAllocationsFromPreference(List<DefaultTimesheetPreference> items)
    {
        var outValue = new List<BatchedOffboardingAllocation>();
        foreach (var item in items)
        {
            var derived = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeBatchedOffboardingAllocation(BatchedOffboardingAllocation item)
    {
        return ScheduledTimesheetScorePolicy.ClassifyScheduledTimesheetScore(item.AdjustedLoad());
    }
}
