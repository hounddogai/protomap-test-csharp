namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Functions of default bonus pool request from milestone.</summary>
public static class DefaultBonusPoolRequestFromMilestone
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

    public static List<DefaultBonusPoolRequest> DeriveDefaultBonusPoolRequestsFromMilestone(List<ScheduledBonusPoolMilestone> items)
    {
        var outValue = new List<DefaultBonusPoolRequest>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolRequest.NewDefaultBonusPoolRequest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDefaultBonusPoolRequest(DefaultBonusPoolRequest item)
    {
        return PrimaryBonusPoolGroupPolicy.PrimaryBonusPoolGroupBucket(item.EffectiveLoad());
    }
}
