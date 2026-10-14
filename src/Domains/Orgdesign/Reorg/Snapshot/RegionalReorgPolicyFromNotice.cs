namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Functions of regional reorg policy from notice.</summary>
public static class RegionalReorgPolicyFromNotice
{
    public static Dictionary<string, double> SummarizeFallbackHeadcountNoticesForPolicy(List<FallbackHeadcountNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RegionalReorgPolicy> DeriveRegionalReorgPoliciesFromNotice(List<FallbackHeadcountNotice> items)
    {
        var outValue = new List<RegionalReorgPolicy>();
        foreach (var item in items)
        {
            var derived = RegionalReorgPolicy.NewRegionalReorgPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreRegionalReorgPolicy(RegionalReorgPolicy item)
    {
        return HeadcountSnapshotPolicy.HeadcountSnapshotWeight(2, true);
    }
}
