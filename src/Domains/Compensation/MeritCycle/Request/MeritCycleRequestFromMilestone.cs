namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Functions of merit cycle request from milestone.</summary>
public static class MeritCycleRequestFromMilestone
{
    public static Dictionary<string, double> SummarizeScheduledBonusPoolMilestonesForRequest(List<ScheduledBonusPoolMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<MeritCycleRequest> DeriveMeritCycleRequestsFromMilestone(List<ScheduledBonusPoolMilestone> items)
    {
        var outValue = new List<MeritCycleRequest>();
        foreach (var item in items)
        {
            var derived = MeritCycleRequest.NewMeritCycleRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateMeritCycleRequest(MeritCycleRequest item)
    {
        return PrimaryBonusPoolGroupPolicy.PrimaryBonusPoolGroupBucket(item.ProjectedLoad());
    }
}
