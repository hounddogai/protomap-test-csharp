namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Functions of merit cycle allocation from milestone.</summary>
public static class MeritCycleAllocationFromMilestone
{
    public static Dictionary<string, double> SummarizeBonusPoolMilestonesForAllocation(List<BonusPoolMilestone> items)
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

    public static List<MeritCycleAllocation> DeriveMeritCycleAllocationsFromMilestone(List<BonusPoolMilestone> items)
    {
        var outValue = new List<MeritCycleAllocation>();
        foreach (var item in items)
        {
            var derived = MeritCycleAllocation.NewMeritCycleAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectMeritCycleAllocation(MeritCycleAllocation item)
    {
        return BonusPoolRequestPolicy.BonusPoolRequestBucket(item.NetLoad());
    }
}
