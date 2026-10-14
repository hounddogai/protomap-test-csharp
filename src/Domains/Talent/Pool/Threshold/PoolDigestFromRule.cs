namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Functions of pool digest from rule.</summary>
public static class PoolDigestFromRule
{
    public static Dictionary<string, double> SummarizeCareerPathRulesForDigest(List<CareerPathRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PoolDigest> DerivePoolDigestsFromRule(List<CareerPathRule> items)
    {
        var outValue = new List<PoolDigest>();
        foreach (var item in items)
        {
            var derived = PoolDigest.NewPoolDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchPoolDigest(PoolDigest item)
    {
        return ScheduledCareerPathProfilePolicy.ScheduledCareerPathProfileWeight(2, true);
    }
}
