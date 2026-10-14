namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Functions of pool mapping from cycle.</summary>
public static class PoolMappingFromCycle
{
    public static Dictionary<string, double> SummarizeSkillProfileCyclesForMapping(List<SkillProfileCycle> items)
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

    public static List<PoolMapping> DerivePoolMappingsFromCycle(List<SkillProfileCycle> items)
    {
        var outValue = new List<PoolMapping>();
        foreach (var item in items)
        {
            var derived = PoolMapping.NewPoolMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePoolMapping(PoolMapping item)
    {
        return PendingSkillProfileDigestPolicy.NormalizePendingSkillProfileDigestCode(item.Code);
    }
}
