namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Functions of capped rate policy from checkpoint.</summary>
public static class CappedRatePolicyFromCheckpoint
{
    public static Dictionary<string, double> SummarizePendingEngagementCheckpointsForPolicy(List<PendingEngagementCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CappedRatePolicy> DeriveCappedRatePoliciesFromCheckpoint(List<PendingEngagementCheckpoint> items)
    {
        var outValue = new List<CappedRatePolicy>();
        foreach (var item in items)
        {
            var derived = CappedRatePolicy.NewCappedRatePolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleCappedRatePolicy(CappedRatePolicy item)
    {
        return EngagementTargetPolicy.NormalizeEngagementTargetCode(item.Code);
    }
}
