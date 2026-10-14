namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Functions of rate milestone from tier.</summary>
public static class RateMilestoneFromTier
{
    public static Dictionary<string, double> SummarizeRateTiersForMilestone(List<RateTier> items)
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

    public static List<RateMilestone> DeriveRateMilestonesFromTier(List<RateTier> items)
    {
        var outValue = new List<RateMilestone>();
        foreach (var item in items)
        {
            var derived = RateMilestone.NewRateMilestone(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRateMilestone(RateMilestone item)
    {
        return RateProfilePolicy.RateProfileBucket(item.WeightedLoad());
    }
}
