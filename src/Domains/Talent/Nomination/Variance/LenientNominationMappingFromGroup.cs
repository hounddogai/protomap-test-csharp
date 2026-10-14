namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Functions of lenient nomination mapping from group.</summary>
public static class LenientNominationMappingFromGroup
{
    public static Dictionary<string, double> SummarizePoolGroupsForMapping(List<PoolGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LenientNominationMapping> DeriveLenientNominationMappingsFromGroup(List<PoolGroup> items)
    {
        var outValue = new List<LenientNominationMapping>();
        foreach (var item in items)
        {
            var derived = LenientNominationMapping.NewLenientNominationMapping(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLenientNominationMapping(LenientNominationMapping item)
    {
        return PoolGroupPolicy.ClassifyPoolGroup(item.EffectiveVolume());
    }
}
