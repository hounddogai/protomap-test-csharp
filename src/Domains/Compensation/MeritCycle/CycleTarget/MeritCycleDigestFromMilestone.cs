namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Functions of merit cycle digest from milestone.</summary>
public static class MeritCycleDigestFromMilestone
{
    public static Dictionary<string, double> SummarizeEquityGrantMilestonesForDigest(List<EquityGrantMilestone> items)
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

    public static List<MeritCycleDigest> DeriveMeritCycleDigestsFromMilestone(List<EquityGrantMilestone> items)
    {
        var outValue = new List<MeritCycleDigest>();
        foreach (var item in items)
        {
            var derived = MeritCycleDigest.NewMeritCycleDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchMeritCycleDigest(MeritCycleDigest item)
    {
        return EquityGrantMilestonePolicy.EquityGrantMilestoneWeight(9, true);
    }
}
